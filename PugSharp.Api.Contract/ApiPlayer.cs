namespace PugSharp.Api.Contract;

public class ApiPlayer : IApiPlayer
{
    public required int Side { get; init; }

    public required bool IsBot { get; init; }

    public required int UserId { get; init; }

    public required ulong SteamId { get; init; }

    public required string Name { get; init; }
}
