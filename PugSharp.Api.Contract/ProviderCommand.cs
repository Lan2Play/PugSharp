
namespace PugSharp.Api.Contract;

public record ProviderCommand(string Name, string Description, Func<string[], IEnumerable<string>> commandCallBack);
