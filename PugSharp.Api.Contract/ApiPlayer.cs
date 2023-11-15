namespace PugSharp.Api.Contract;

public class ApiPlayer : IApiPlayer
{
    public int Side { get; init; }

    public bool IsBot { get; init; }

    public int UserId { get; init; }

    public ulong SteamId { get; init; }

    public string Name { get; init; }
}
