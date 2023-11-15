
namespace PugSharp.Server.Contract;

public interface ICsServer
{
    string GameDirectory { get; }

    void ExecuteCommand(string v);
    bool IsMapValid(string selectedMap);
    (int CtScore, int TScore) LoadTeamsScore();

    void NextFrame(Action value);
    void PrintToChatAll(string message);
}
