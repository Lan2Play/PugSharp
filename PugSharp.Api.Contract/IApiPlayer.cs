namespace PugSharp.Api.Contract;

public interface IApiPlayer
{
    int Side { get; }

    bool IsBot { get; }

    int UserId { get; }

    ulong SteamId { get; }

    string Name { get; }
}