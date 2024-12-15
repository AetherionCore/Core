using GameServerCore.Domain;

namespace LeagueSandbox.GameServer.Chatbox
{
    public interface IChatCommand : IUpdate
    {
        string Command { get; }
        string[] AlternativeCommands { get; }
        string Syntax { get; }
        void Execute(int userId, bool hasReceivedArguments, string arguments = "");
        void ShowSyntax(int userId);
    }
}
