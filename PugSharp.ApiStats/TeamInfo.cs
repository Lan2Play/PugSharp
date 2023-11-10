using PugSharp.Api.Contract;
using PugSharp.Match.Contract;

namespace PugSharp.ApiStats
{
    public class TeamInfo : ITeamInfo
    {
        public string Id { get; set; }

        public string TeamName { get; set; }
    }
}
