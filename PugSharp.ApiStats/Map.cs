namespace PugSharp.ApiStats
{
    public class Map
    {
        public string Name { get; set; }

        public string DemoFileName { get; set; }

        public Config.Team Winner { get; set; }

        public MapTeamInfo Team1 { get; set; }

        public MapTeamInfo Team2 { get; set; }
    }
}
