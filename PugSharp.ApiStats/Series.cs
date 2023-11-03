namespace PugSharp.ApiStats
{
    public class Series
    {
        public Dictionary<string, Map> Maps { get; set; }

        public TeamInfo Team1 { get; set; }

        public TeamInfo Team2 { get; set; }

        public string SeriesType => $"bo{Maps.Count}";

        public Config.Team Winner { get; set; }

        public bool Forfeit { get; set; }

        public Map GetMap(int mapNumber)
        {
            var mapNumberString = $"map{mapNumber}";

            return Maps[mapNumberString];
        }
    }
}
