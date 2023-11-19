namespace PugSharp;

/// <summary>
/// Application interface
/// </summary>
public interface IApplication : IDisposable
{
    /// <summary>
    /// Initialize event registrations and more
    /// </summary>
    void Initialize(bool hotReload);
}