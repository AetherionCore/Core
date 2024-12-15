using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions;
using LeagueSandbox.GameServer.Logging;
using log4net;
using PacketDefinitions420;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace LeagueSandbox.GameServer
{
    /// <summary>
    /// Class which controls the starting of the game and network loops.
    /// </summary>
    internal class Server : IDisposable
    {
        private string[] _blowfishKeys;
        private static ILog _logger = LoggerProvider.GetLogger();
        private Game _game;
        private Config _config;
        private ushort _serverPort { get; }

        /// <summary>
        /// Initialize base variables for future usage.
        /// </summary>
        public Server(Game game, ushort port, string configJson)
        {
            _game = game;
            _serverPort = port;
            _config = Config.LoadFromJson(game, configJson);

            _blowfishKeys = new string[_config.Players.Count];
            for (int i = 0; i < _config.Players.Count; i++)
            {
                _blowfishKeys[i] = _config.Players[i].BlowfishKey;
            }
        }

        private void ShowBanner()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(@" __       ____    ______  ____    __  __  ____        ____    ____    ____    __  __  ____    ____       ");
            Console.WriteLine(@"/\ \     /\  _`\ /\  _  \/\  _`\ /\ \/\ \/\  _`\     /\  _`\ /\  _`\ /\  _`\ /\ \/\ \/\  _`\ /\  _`\     ");
            Console.WriteLine(@"\ \ \    \ \ \L\_\ \ \L\ \ \ \L\_\ \ \ \ \ \ \L\_\   \ \,\L\_\ \ \L\_\ \ \L\ \ \ \ \ \ \ \L\_\ \ \L\ \   ");
            Console.WriteLine(@" \ \ \  __\ \  _\L\ \  __ \ \ \L_L\ \ \ \ \ \  _\L    \/_\__ \\ \  _\L\ \ ,  /\ \ \ \ \ \  _\L\ \ ,  /   ");
            Console.WriteLine(@"  \ \ \L\ \\ \ \L\ \ \ \/\ \ \ \/, \ \ \_\ \ \ \L\ \    /\ \L\ \ \ \L\ \ \ \\ \\ \ \_/ \ \ \L\ \ \ \\ \  ");
            Console.WriteLine(@"   \ \____/ \ \____/\ \_\ \_\ \____/\ \_____\ \____/    \ `\____\ \____/\ \_\ \_\ `\___/\ \____/\ \_\ \_\");
            Console.WriteLine(@"    \/___/   \/___/  \/_/\/_/\/___/  \/_____/\/___/      \/_____/\/___/  \/_/\/ /`\/__/  \/___/  \/_/\/ /");
            Console.WriteLine();
            Console.WriteLine();
            Console.ResetColor();
        }

        /// <summary>
        /// Called upon the Program successfully initializing GameServerLauncher.
        /// </summary>
        public void Start()
        {
            var build = $"Core rev. {ServerContext.GitCommitHash} @ {ServerContext.BuildDateString}";
            var packetServer = new PacketServer();

            Console.Title = "AetherionCore GameServer";

            ShowBanner();
            _logger.Info(build);
            _logger.Info($"Game started on port: {_serverPort}");

            packetServer.InitServer(_serverPort, _blowfishKeys, _game, _game.RequestHandler, _game.ResponseHandler);
            _game.Initialize(_config, packetServer);
        }

        /// <summary>
        /// Called after the Program has finished setting up the Server for players to join.
        /// </summary>
        public void StartNetworkLoop()
        {
            _game.GameLoop();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. Unused.
        /// </summary>
        public void Dispose()
        {
            // PathNode.DestroyTable();
        }
    }
}