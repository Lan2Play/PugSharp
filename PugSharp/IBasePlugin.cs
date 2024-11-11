using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Timers;

namespace PugSharp;

/// <summary>
/// Example plugin interface
/// </summary>
public interface IBasePlugin
{
    /// <summary>
    /// Module name that will be displayed in CSSharp
    /// </summary>
    string ModuleName { get; }

    /// <summary>
    /// Module description that will be displayed in CSSharp
    /// </summary>
    string ModuleDescription { get; }

    /// <summary>
    /// Module version that will be displayed in CSSharp
    /// </summary>
    string ModuleVersion { get; }

    /// <summary>
    /// Module author that will be displayed in CSSharp
    /// </summary>
    string ModuleAuthor { get; }

    /// <summary>
    /// Module path on the server
    /// </summary>
    string ModulePath { get; }

    /// <summary>
    /// Module folder path on the server
    /// </summary>
    string ModuleDirectory { get; }

    /// <summary>
    /// Register game event handler with CSSharp
    /// </summary>
    /// <typeparam name="T">CounterStrikeSharp.API.Modules.Events.GameEvent</typeparam>
    /// <param name="handler">Callback</param>
    /// <param name="hookMode">Hook mode</param>
    void RegisterEventHandler<T>(BasePlugin.GameEventHandler<T> handler, HookMode hookMode = HookMode.Post)
        where T : GameEvent;

    /// <summary>
    /// Deregister a game event handler with CSSharp
    /// </summary>
    /// <param name="name">Name of the event</param>
    /// <param name="handler">Callback</param>
    /// <param name="post">Hook mode is post</param>
    void DeregisterEventHandler(string name, Delegate handler, bool post);

    /// <summary>
    /// Add console command
    /// </summary>
    /// <param name="name">Console command name</param>
    /// <param name="description">Console command description</param>
    /// <param name="handler">Callback</param>
    void AddCommand(string name, string description, CommandInfo.CommandCallback handler);

    /// <summary>
    /// Add console command listener
    /// </summary>
    /// <param name="name">Console command name</param>
    /// <param name="handler">Callback</param>
    /// <param name="mode">Hook mode</param>
    void AddCommandListener(string? name, CommandInfo.CommandListenerCallback handler, HookMode mode = HookMode.Pre);

    /// <summary>
    /// Remove console command
    /// </summary>
    /// <param name="name">Console command name</param>
    /// <param name="handler">Callback</param>
    void RemoveCommand(string name, CommandInfo.CommandCallback handler);

    /// <summary>
    /// Remove console command listener
    /// </summary>
    /// <param name="name">Console command name</param>
    /// <param name="handler">Callback</param>
    /// <param name="mode">Hook mode</param>
    void RemoveCommandListener(string name, CommandInfo.CommandListenerCallback handler, HookMode mode);

    /// <summary>
    /// Register listener
    /// </summary>
    /// <typeparam name="T">CounterStrikeSharp.API.Core.Listeners</typeparam>
    /// <param name="handler">Callback</param>
    void RegisterListener<T>(T handler)
        where T : Delegate;

    /// <summary>
    /// Remove listener
    /// </summary>
    /// <param name="name">Listener name</param>
    /// <param name="handler">Callback</param>
    void RemoveListener(string name, Delegate handler);

    /// <summary>
    /// Add timer
    /// </summary>
    /// <param name="interval">Interval tick</param>
    /// <param name="callback">Callback</param>
    /// <param name="flags">Flags</param>
    /// <returns></returns>
    CounterStrikeSharp.API.Modules.Timers.Timer AddTimer(float interval, Action callback, TimerFlags? flags = null);

    /// <summary>
    /// Register all attributes in instances
    /// </summary>
    /// <param name="instance">Class reference</param>
    void RegisterAllAttributes(object instance);

    /// <summary>
    /// Initialize config
    /// </summary>
    /// <param name="instance">Class reference</param>
    /// <param name="pluginType">Type of plugin</param>
    void InitializeConfig(object instance, Type pluginType);

    /// <summary>
    /// Register attribute handlers
    /// </summary>
    /// <param name="instance">Class reference</param>
    void RegisterAttributeHandlers(object instance);

    /// <summary>
    /// Register console command attribute handlers
    /// </summary>
    /// <param name="instance">Class reference</param>
    void RegisterConsoleCommandAttributeHandlers(object instance);
}
