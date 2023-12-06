using System.Runtime.InteropServices;

using CounterStrikeSharp.API.Core;

using PugSharp.Server.Contract;

namespace PugSharp;

public class CssDispatcher : ICssDispatcher
{
    private int _TaskHandle;
    private readonly Dictionary<int, Action> _NextFrameTasks = new();

    private void AutoRemoveAction(int handle, Action action)
    {
        action();
        _NextFrameTasks.Remove(handle);
    }

#pragma warning disable IDE0039 // Use local function
    public void NextFrame(Action action)
    {
        var handle = Interlocked.Increment(ref _TaskHandle);
        var autoRemoveAction = () => AutoRemoveAction(handle, action);
        _NextFrameTasks.Add(handle, autoRemoveAction);

        var ptr = Marshal.GetFunctionPointerForDelegate(autoRemoveAction);
        NativeAPI.QueueTaskForNextFrame(ptr);
    }

    public void NextWorldUpdate(Action action)
    {
        var handle = Interlocked.Increment(ref _TaskHandle);
        var autoRemoveAction = () => AutoRemoveAction(handle, action);
        _NextFrameTasks.Add(handle, autoRemoveAction);

        var ptr = Marshal.GetFunctionPointerForDelegate(autoRemoveAction);
        NativeAPI.QueueTaskForNextWorldUpdate(ptr);
    }

#pragma warning restore IDE0039 // Use local function
}
