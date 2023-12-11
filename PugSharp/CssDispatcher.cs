using System.Runtime.InteropServices;

using CounterStrikeSharp.API.Core;

using Microsoft.Extensions.Logging;

using PugSharp.Server.Contract;

namespace PugSharp;

public class CssDispatcher : ICssDispatcher
{
    private int _TaskHandle;
    private readonly Dictionary<int, Action> _NextFrameTasks = new();
    private readonly ILogger<CssDispatcher> _Logger;

    public CssDispatcher(ILogger<CssDispatcher> logger)
    {
        _Logger = logger;
    }

    private void AutoRemoveAction(int handle, Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            _Logger.LogError(ex, "Error executing auto remove action!");
        }
        _NextFrameTasks.Remove(handle);
    }

#pragma warning disable IDE0039 // Use local function

    public void NextFrame(Action action)
    {
        var localAction = action;
        var handle = Interlocked.Increment(ref _TaskHandle);
        var autoRemoveAction = () => AutoRemoveAction(handle, localAction);
        _NextFrameTasks.Add(handle, autoRemoveAction);

        var ptr = Marshal.GetFunctionPointerForDelegate(autoRemoveAction);
        NativeAPI.QueueTaskForNextFrame(ptr);
    }

    public void NextWorldUpdate(Action action)
    {
        var localAction = action;
        var handle = Interlocked.Increment(ref _TaskHandle);
        var autoRemoveAction = () => AutoRemoveAction(handle, localAction);
        _NextFrameTasks.Add(handle, autoRemoveAction);

        var ptr = Marshal.GetFunctionPointerForDelegate(autoRemoveAction);
        NativeAPI.QueueTaskForNextWorldUpdate(ptr);
    }

#pragma warning restore IDE0039 // Use local function
}
