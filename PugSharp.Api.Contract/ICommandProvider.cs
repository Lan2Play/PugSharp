
namespace PugSharp.Api.Contract;

public interface ICommandProvider
{
    IReadOnlyList<ProviderCommand> LoadProviderCommands();
}
