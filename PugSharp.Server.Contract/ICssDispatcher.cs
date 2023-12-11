
namespace PugSharp.Server.Contract;

public interface ICssDispatcher
{
    void NextFrame(Action action);
    void NextWorldUpdate(Action action);
}
