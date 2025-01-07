namespace PugSharp.Match.Contract;

public interface IPlayer
{
    ulong SteamID { get; }

    int? UserId { get; }

    string PlayerName { get; }

    Team Team { get; }
    string Clan { get; set; }

    void PrintToChat(string message);

    void SwitchTeam(Team team);

    void ShowMenu(string title, IEnumerable<MenuOption> menuOptions);

    void Kick();
}
