﻿using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class ManaCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "mana";
        public override string Syntax => $"{Command} maxMana";

        public ManaCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, userId: userId);
                ShowSyntax(userId);
            }
            else if (float.TryParse(split[1], out var mp))
            {
                _playerManager.GetPeerInfo(userId).Champion.Stats.ManaPoints.FlatBonus += mp;
                _playerManager.GetPeerInfo(userId).Champion.Stats.CurrentMana += mp;
            }
        }
    }
}
