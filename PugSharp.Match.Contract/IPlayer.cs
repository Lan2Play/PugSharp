using System.Text.Json.Serialization;

namespace PugSharp.Match.Contract;

public interface IPlayer
{
    ulong SteamID { get; }

    int? UserId { get; }

    IPlayerPawn PlayerPawn { get; }

    string PlayerName { get; }

    IPlayerMatchStats? MatchStats { get; }
    Team Team { get; }

    void PrintToChat(string message);

    void SwitchTeam(Team team);

    void ShowMenu(string title, IEnumerable<MenuOption> menuOptions);
}
