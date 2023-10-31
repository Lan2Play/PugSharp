namespace PugSharp.Match.Contract
{
    public class MenuOption
    {
        public string DisplayName { get; }

        public Action<MenuOption, IPlayer> Action { get; }

        public MenuOption(string displayName, Action<MenuOption, IPlayer> action)
        {
            DisplayName = displayName;
            Action = action;
        }
    }
}
