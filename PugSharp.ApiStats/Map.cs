using PugSharp.Match.Contract;

namespace PugSharp.ApiStats
{
    public class Map : IMap
    {
        public string Name { get; set; }

        public string DemoFileName { get; set; }

        public string WinnerTeamName { get; set; }

        public IMapTeamInfo Team1 { get; set; }

        public IMapTeamInfo Team2 { get; set; }
    }
}
