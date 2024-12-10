﻿using GameServerCore.Enums;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Chatbox;
using LeagueSandbox.GameServer.Logging;
using LeagueSandbox.GameServer.Packets;
using LeagueSandbox.GameServer.Players;
using LeagueSandbox.GameServer.Scripting.CSharp;
using log4net;
using LeagueSandbox.GameServer.Inventory;
using PacketDefinitions420;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Timer = System.Timers.Timer;
using LeagueSandbox.GameServer.Packets.PacketHandlers;
using LeagueSandbox.GameServer.Handlers;
using GameServerCore.Packets.PacketDefinitions;
using GameServerCore.Packets.PacketDefinitions.Requests;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using GameServerLib.Handlers;
using GameServerLib.Scripting;

namespace LeagueSandbox.GameServer
{
    /// <summary>
    /// Class that contains and manages all qualities of the game such as managers for networking and game mechanics, as well as the starting, pausing, and stopping of the game.
    /// </summary>
    public class Game
    {
        // Crucial Game Vars
        private PacketServer _packetServer;
        private List<GameScriptTimer> _gameScriptTimers;

        // Function Vars
        private static ILog _logger = LoggerProvider.GetLogger();
        private float _nextSyncTime = 10 * 1000;
        protected const double REFRESH_RATE = 1000.0 / 60.0f; // GameLoop called 60 times a second.
        private HandleStartGame _gameStartHandler;

        // Server

        /// <summary>
        /// Whether the server is running or not. Usually true after the network loop has started via GameServerLauncher.
        /// </summary>
        public bool IsRunning { get; private set; }
        /// <summary>
        /// Whether or not the game has been paused (via a chat command usually).
        /// </summary>
        public bool IsPaused { get; set; }
        /// <summary>
        /// Time until the game unpauses (if paused).
        /// </summary>
        public long PauseTimeLeft { get; private set; }
        /// <summary>
        /// Whether or not the game is set as finished (and thus whether the server should close).
        /// </summary>
        public bool SetToExit { get; set; }

        // Networking

        /// <summary>
        /// Time since the game has started. Mostly used for networking to sync up players with the server.
        /// </summary>
        public float GameTime { get; private set; }

        /// <summary>
        /// Last Frame DeltaTime
        /// </summary>
        public float FrameDelta { get; private set; }

        /// <summary>
        /// Handler for request packets sent by game clients.
        /// </summary>
        public NetworkHandler<ICoreRequest> RequestHandler { get; }
        /// <summary>
        /// Handler for response packets sent by the server to game clients.
        /// </summary>
        public NetworkHandler<ICoreRequest> ResponseHandler { get; }
        /// <summary>
        /// Interface containing all function related packets (except handshake) which are sent by the server to game clients.
        /// </summary>
        public PacketNotifier PacketNotifier { get; private set; }

        // Game

        /// <summary>
        /// Interface containing all (public) functions used by ObjectManager. ObjectManager manages GameObjects, their properties, and their interactions such as being added, removed, colliding with other objects or terrain, vision, teams, etc.
        /// </summary>
        public ObjectManager ObjectManager { get; private set; }
        /// <summary>
        /// Interface for all protection related functions.
        /// Protection is a mechanic which determines whether or not a unit is targetable.
        /// </summary>
        public ProtectionManager ProtectionManager { get; private set; }
        /// <summary>
        /// Contains all map related game settings such as collision handler, navigation grid, announcer events, and map properties. Doubles as a Handler/Manager for all MapScripts.
        /// </summary>
        public MapScriptHandler Map { get; private set; }
        /// <summary>
        /// Class containing all information about the game's configuration such as game content location, map spawn points, whether cheat commands are enabled, etc.
        /// </summary>
        public Config Config { get; protected set; }
        /// <summary>
        /// Class which manages items of players.
        /// </summary>
        public ItemManager ItemManager { get; private set; }
        /// <summary>
        /// Class which manages all chat based commands.
        /// </summary>
        internal ChatCommandManager ChatCommandManager { get; private set; }
        /// <summary>
        /// Interface of functions used to identify players or their properties (such as their champion).
        /// </summary>
        public PlayerManager PlayerManager { get; private set; }
        /// <summary>
        /// Manager for all unique identifiers used by GameObjects.
        /// </summary>
        internal NetworkIdManager NetworkIdManager { get; private set; }
        /// <summary>
        /// Class that compiles and loads all scripts which will be used for the game (ex: spells, items, AI, maps, etc).
        /// </summary>
        internal static CSharpScriptEngine ScriptEngine { get; private set; }

        internal FileSystemWatcher ScriptsHotReloadWatcher { get; private set; }

        /// <summary>
        /// Instantiates all game managers and handlers.
        /// </summary>
        public Game()
        {
            ItemManager = new ItemManager();
            ChatCommandManager = new ChatCommandManager(this);
            NetworkIdManager = new NetworkIdManager();
            PlayerManager = new PlayerManager(this);
            ScriptEngine = new CSharpScriptEngine();
            RequestHandler = new NetworkHandler<ICoreRequest>();
            ResponseHandler = new NetworkHandler<ICoreRequest>();
        }

        /// <summary>
        /// Sets up all managers and config specific settings like players.
        /// </summary>
        /// <param name="config">Game configuration file. Usually from GameInfo.json.</param>
        /// <param name="server">Server networking instance.</param>
        public void Initialize(Config config, PacketServer server)
        {
            _logger.Info("Loading Config.");
            Config = config;
            Config.LoadContent(this);
            AssemblyService.TryLoadAssemblies(Config.AssemblyNames);
            _gameScriptTimers = new List<GameScriptTimer>();

            ChatCommandManager.LoadCommands();

            Map = new MapScriptHandler(this);

            // TODO: GameApp should send the Response/Request handlers
            _packetServer = server;
            // TODO: switch the notifier with ResponseHandler
            PacketNotifier = new PacketNotifier(_packetServer.PacketHandlerManager, Map.NavigationGrid);

            ObjectManager = new ObjectManager(this);
            ProtectionManager = new ProtectionManager(this);
            ApiGameEvents.SetGame(this);
            ApiMapFunctionManager.SetGame(this, Map as MapScriptHandler);
            ApiFunctionManager.SetGame(this);
            ApiEventManager.SetGame(this);
            ChampionDeathHandler.Init(this);
            IsRunning = false;

            Map.Init();

            PauseTimeLeft = 30 * 60; // 30 minutes

            InitializePacketHandlers();

            _logger.Info("Add players");
            foreach (var p in Config.Players)
            {
                _logger.Info("Player " + p.Name + " Added: " + p.Champion);
                PlayerManager.AddPlayer(p);
            }

            _logger.Info("Game is ready.");
        }

        /// <summary>
        /// Registers Request Handlers for each request packet.
        /// </summary>
        public void InitializePacketHandlers()
        {
            // maybe use reflection, the problem is that Register is generic and so it needs to know its type at
            // compile time, maybe just use interface and in runetime figure out the type - and again there is
            // a problem with passing generic delegate to non-generic function, if we try to only constraint the
            // argument to interface ICoreRequest we will get an error cause our generic handlers use generic type
            // even with where statement that doesn't work
            RequestHandler.Register<AttentionPingRequest>(new HandleAttentionPing(this).HandlePacket);
            RequestHandler.Register<AutoAttackOptionRequest>(new HandleAutoAttackOption(this).HandlePacket);
            RequestHandler.Register<BlueTipClickedRequest>(new HandleBlueTipClicked(this).HandlePacket);
            RequestHandler.Register<BuyItemRequest>(new HandleBuyItem(this).HandlePacket);
            RequestHandler.Register<CastSpellRequest>(new HandleCastSpell(this).HandlePacket);
            RequestHandler.Register<ChatMessageRequest>(new HandleChatBoxMessage(this).HandlePacket);
            RequestHandler.Register<ClickRequest>(new HandleClick(this).HandlePacket);
            RequestHandler.Register<SpellChargeUpdateReq>(new HandleSpellChargeUpdateReq(this).HandlePacket);
            RequestHandler.Register<EmotionPacketRequest>(new HandleEmotion(this).HandlePacket);
            RequestHandler.Register<ExitRequest>(new HandleExit(_packetServer.PacketHandlerManager).HandlePacket);
            RequestHandler.Register<SyncSimTimeRequest>(new HandleSyncSimTime(this).HandlePacket);
            RequestHandler.Register<PingLoadInfoRequest>(new HandleLoadPing(this).HandlePacket);
            RequestHandler.Register<LockCameraRequest>(new HandleLockCamera(this).HandlePacket);
            RequestHandler.Register<JoinTeamRequest>(new HandleJoinTeam(this).HandlePacket);
            RequestHandler.Register<MovementRequest>(new HandleMove(this).HandlePacket);
            RequestHandler.Register<MoveConfirmRequest>(new HandleMoveConfirm(this).HandlePacket);
            RequestHandler.Register<PauseRequest>(new HandlePauseReq(this).HandlePacket);
            RequestHandler.Register<QueryStatusRequest>(new HandleQueryStatus(this).HandlePacket);
            RequestHandler.Register<QuestClickedRequest>(new HandleQuestClicked(this).HandlePacket);
            RequestHandler.Register<ScoreboardRequest>(new HandleScoreboard(this).HandlePacket);
            RequestHandler.Register<SellItemRequest>(new HandleSellItem(this).HandlePacket);
            RequestHandler.Register<UpgradeSpellReq>(new HandleUpgradeSpellReq(this).HandlePacket);
            RequestHandler.Register<SpawnRequest>(new HandleSpawn(this).HandlePacket);

            _gameStartHandler = new HandleStartGame(this);
            RequestHandler.Register<StartGameRequest>(_gameStartHandler.HandlePacket);

            RequestHandler.Register<ReplicationConfirmRequest>(new HandleStatsConfirm(this).HandlePacket);
            RequestHandler.Register<SurrenderRequest>(new HandleSurrender(this).HandlePacket);
            RequestHandler.Register<SwapItemsRequest>(new HandleSwapItems(this).HandlePacket);
            RequestHandler.Register<SynchVersionRequest>(new HandleSync(this).HandlePacket);
            RequestHandler.Register<UnpauseRequest>(new HandleUnpauseReq(this).HandlePacket);
            RequestHandler.Register<UseObjectRequest>(new HandleUseObject(this).HandlePacket);
            RequestHandler.Register<ViewRequest>(new HandleView(this).HandlePacket);
        }

        /// <summary>
        /// Enables or disables the hot reloading of scripts. Used only for development.
        /// </summary>
        public void EnableHotReload(bool status)
        {
            string scriptsPath = Config.ContentManager.ContentPath;

            void ScriptsChanged(object _, FileSystemEventArgs ea)
            {
                // Disable raising events to avoid triggering LoadScripts() many times in a row after the first event
                ScriptsHotReloadWatcher.EnableRaisingEvents = false;
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, LoadScripts() ? "Scripts reloaded." : "Scripts failed to reload.");
                ScriptsHotReloadWatcher.EnableRaisingEvents = true;
            }

            if (status && ScriptsHotReloadWatcher == null)
            {
                ScriptsHotReloadWatcher = new FileSystemWatcher
                {
                    Path = scriptsPath,
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true,
                    NotifyFilter = NotifyFilters.LastWrite,
                    Filter = "*.*",
                };
                ScriptsHotReloadWatcher.Changed += ScriptsChanged;
            }
            else
            {
                ScriptsHotReloadWatcher.Changed -= ScriptsChanged;
                ScriptsHotReloadWatcher = null;
            }
        }

        /// <summary>
        /// Loads the scripts contained in every content package.
        /// </summary>
        /// <returns>Whether all scripts were loaded successfully or not.</returns>
        public bool LoadScripts()
        {
            bool scriptLoadingResults = Config.ContentManager.LoadScripts();

            if (scriptLoadingResults)
            {
                foreach (var unit in ObjectManager.GetObjects().Values)
                {
                    if (unit is ObjAIBase obj)
                    {
                        if (obj.Spells.ContainsKey((int)SpellSlotType.PassiveSpellSlot))
                        {
                            obj.LoadCharScript(obj.Spells[(int)SpellSlotType.PassiveSpellSlot]);
                        }
                        else
                        {
                            obj.LoadCharScript();
                        }
                        obj.GetBuffs().ForEach(buff => buff.LoadScript());
                        obj.Spells.Values.ToList().ForEach(spell => spell.LoadScript());
                    }
                }
            }

            return scriptLoadingResults;
        }

        public bool CheckIfAllPlayersLeft()
        {
            var players = PlayerManager.GetPlayers(false);
            // The number of those who are disconnected and not even loads.
            var count = players.Count(p => !p.IsStartedClient && p.IsDisconnected);
            Console.WriteLine($"The number of disconnected players {count}/{players.Count}");
            if (count == players.Count)
            {
                _logger.Info("All players have left the server. Server exit.");
                SetToExit = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Function which initiates ticking of the game's logic.
        /// </summary>
        public void GameLoop()
        {
            double refreshRate = REFRESH_RATE;
            double timeout = 0;

            Stopwatch lastMapDurationWatch = new Stopwatch();

            bool wasNotPaused = true;
            bool firstCycle = true;

            float timeToForcedStart = Config.ForcedStart;

            while (!SetToExit)
            {
                double lastSleepDuration = lastMapDurationWatch.Elapsed.TotalMilliseconds;
                lastMapDurationWatch.Restart();

                float deltaTime = (float)lastSleepDuration;
                FrameDelta = deltaTime;

                if (firstCycle)
                {
                    firstCycle = false;
                    // To avoid Update(0)
                    deltaTime = (float)refreshRate;
                }

                if (IsPaused)
                {
                    if (wasNotPaused)
                    {
                        refreshRate = 1000.0;
                        wasNotPaused = false;
                    }
                    else
                    {
                        PauseTimeLeft--;
                        if (PauseTimeLeft <= 0)
                        {
                            //TODO: fix these
                            //PacketNotifier.NotifyUnpauseGame();

                            // Pure water framing
                            var players = PlayerManager.GetPlayers();
                            var unpauser = players[0].Champion;
                            foreach (var player in players)
                            {
                                PacketNotifier.NotifyResumePacket(unpauser, player, false);
                            }
                            Unpause();
                        }
                    }
                }

                if (!IsPaused)
                {
                    refreshRate = REFRESH_RATE;
                    wasNotPaused = true;

                    if (!IsRunning && timeToForcedStart > 0)
                    {
                        if (timeToForcedStart <= deltaTime && !CheckIfAllPlayersLeft())
                        {
                            _logger.Info($"Patience is over. The game will start earlier.");
                            _gameStartHandler.ForceStart();
                        }
                        timeToForcedStart -= deltaTime;
                    }

                    if (IsRunning)
                    {
                        Update(deltaTime);
                    }
                }
                double lastUpdateDuration = lastMapDurationWatch.Elapsed.TotalMilliseconds;
                double oversleep = lastSleepDuration - timeout;
                timeout = Math.Max(0, refreshRate - lastUpdateDuration - oversleep);

                _packetServer.NetLoop((uint)timeout);
            }
        }

        /// <summary>
        /// Function called every tick of the game.
        /// </summary>
        /// <param name="diff">Number of milliseconds since this tick occurred.</param>
        public void Update(float diff)
        {
            // This section dictates the priority of updates.
            GameTime += diff;
            // Collision
            Map.Update(diff);
            // Objects
            var watch = Stopwatch.StartNew();
            ObjectManager.Update(diff);
            watch.Stop();
            //if (watch.Elapsed.TotalMilliseconds > 5)
            //    _logger.Info($"Object Manager Update took {watch.Elapsed.TotalMilliseconds}ms");
            // Protection (TODO: Move this into ObjectManager).
            ProtectionManager.Update(diff);
            ChatCommandManager.GetCommands().ForEach(command => command.Update(diff));
            _gameScriptTimers.ForEach(gsTimer => gsTimer.Update(diff));
            _gameScriptTimers.RemoveAll(gsTimer => gsTimer.IsDead());

            // By default, synchronize the game time between server and clients every 10 seconds
            _nextSyncTime += diff;
            if (_nextSyncTime >= 10 * 1000)
            {
                PacketNotifier.NotifySynchSimTimeS2C(GameTime);
                _nextSyncTime = 0;
            }
        }

        /// <summary>
        /// Adds a timer to the list of timers so that it ticks with the game.
        /// </summary>
        /// <param name="timer">Timer instance.</param>
        public void AddGameScriptTimer(GameScriptTimer timer)
        {
            _gameScriptTimers.Add(timer);
        }

        /// <summary>
        /// Removes a timer from the list of timers which causes it to become inactive.
        /// </summary>
        /// <param name="timer">Timer instance.</param>
        public void RemoveGameScriptTimer(GameScriptTimer timer)
        {
            _gameScriptTimers.Remove(timer);
        }

        /// <summary>
        /// Function to set the game as running. Allows the game loop to start.
        /// </summary>
        public void Start()
        {
            IsRunning = true;
            try
            {
                Map.MapScript.OnMatchStart();
            }
            catch (Exception e)
            {
                _logger.Error(null, e);
            }
        }

        /// <summary>
        /// Function to set the game as not running. Prevents the game loop from continuing.
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
        }

        /// <summary>
        /// Temporarily prevents the game loop from continuing and notifies players.
        /// </summary>
        public void Pause()
        {
            if (PauseTimeLeft <= 0)
            {
                return;
            }
            IsPaused = true;
            foreach (var player in PlayerManager.GetPlayers(false))
            {
                PacketNotifier.NotifyPausePacket(player, (int)PauseTimeLeft, true);
            }
        }

        /// <summary>
        /// Releases the game loop from a temporary pause.
        /// </summary>
        public void Unpause()
        {
            IsPaused = false;
        }

        /// <summary>
        /// Unused function meant to get the instances of a specific type who rely on Game as a parameter.
        /// </summary>
        /// <returns>List of instances of type T.</returns>
        private static List<T> GetInstances<T>(Game g)
        {
            return Assembly.GetCallingAssembly()
                .GetTypes()
                .Where(t => t.BaseType == typeof(T))
                .Select(t => (T)Activator.CreateInstance(t, g)).ToList();
        }

        /// <summary>
        /// Prepares to close the Game 10 seconds after being called.
        /// </summary>
        public void SetGameToExit()
        {
            _logger.Info("Game is over. Game Server will exit in 10 seconds.");
            var timer = new Timer(10000) { AutoReset = false };
            timer.Elapsed += (a, b) => SetToExit = true;
            timer.Start();
        }
    }
}
