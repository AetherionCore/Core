using LeagueSandbox.GameServer.Players;
using System.Linq;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class TpCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "tp";
        public override string Syntax => $"{Command} [target NetID (0 or none for self)] X Y";

        public TpCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.Split(' ');

            uint targetNetID = 0;
            float x, y;

            if (split.Length < 2 || split.Length > 4)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, userId: userId);
                ShowSyntax(userId);
                return;
            }

            if (!split[1].All(char.IsDigit))
            {
                var pplayer = _playerManager.GetPeerInfo(userId);

                foreach(var player in _playerManager.GetPlayers(false))
                {
                    if (player.Champion != null && !player.Champion.IsDead && player.Champion.CharData.Name == split[1])
                    {
                        if (split.Length > 2 && split[2] == "e" && player.Team != pplayer?.Team)
                        {
                            player.Champion.StopMovement();
                            player.Champion.TeleportTo(pplayer.Champion.Position);
                        }
                        else if (split.Length > 2 && split[2] == "a" && player.Team == pplayer?.Team)
                        {
                            player.Champion.StopMovement();
                            player.Champion.TeleportTo(pplayer.Champion.Position);
                        }
                        else
                        { // don't care about team / not provided
                            player.Champion.StopMovement();
                            player.Champion.TeleportTo(pplayer.Champion.Position);
                        }
                    }
                }
            }

            if (split.Length > 3 && uint.TryParse(split[1], out targetNetID) && float.TryParse(split[2], out x) && float.TryParse(split[3], out y))
            {
                var obj = Game.ObjectManager.GetObjectById(targetNetID);
                if (obj != null)
                {
                    obj.TeleportTo(x, y);
                }
                else
                {
                    ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, "An object with the netID: " + targetNetID + " was not found.", userId);
                    ShowSyntax(userId);
                }
            }
            else if (float.TryParse(split[1], out x) && float.TryParse(split[2], out y))
            {
                _playerManager.GetPeerInfo(userId).Champion.TeleportTo(x, y);
            }
        }
    }
}