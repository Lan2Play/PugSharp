using Microsoft.Extensions.Logging;

using PugSharp.Server.Contract;

namespace PugSharp;

public class CssDispatcher : ICssDispatcher
{
    private readonly ILogger<CssDispatcher> _Logger;

    public CssDispatcher(ILogger<CssDispatcher> logger)
    {
        _Logger = logger;
    }

    public void NextFrame(Action action)
    {
        CounterStrikeSharp.API.Server.NextFrame(action);


    }

    public void NextWorldUpdate(Action action)
    {
        CounterStrikeSharp.API.Server.NextWorldUpdate(action);
    }
}
