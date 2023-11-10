using PugSharp.Api.Contract;
using PugSharp.G5Api;
using static PugSharp.PugSharp;

namespace PugSharp;

public class G5ApiProvider : IApiProvider
{
    private G5ApiClient _G5Stats;

    public G5ApiProvider(G5ApiClient apiClient)
    {
        _G5Stats = apiClient;
    }


    public Task FinalizeMapAsync(MapResultParams finalizeMapParams, CancellationToken cancellationToken)
    {
        var emptyStatsTeam = new StatsTeam(string.Empty, string.Empty, 0, 0, 0, 0, Enumerable.Empty<StatsPlayer>());
        return _G5Stats.SendEventAsync(new MapResultEvent(finalizeMapParams.MatchId, finalizeMapParams.MapNumber, new Winner((Side)(int)Utils.LoadMatchWinner(), finalizeMapParams.Team1Score > finalizeMapParams.Team2Score ? 1 : 2), emptyStatsTeam, emptyStatsTeam), cancellationToken);
    }

    public Task GoingLiveAsync(GoingLiveParams goingLiveParams, CancellationToken cancellationToken)
    {
        return _G5Stats.SendEventAsync(new GoingLiveEvent(goingLiveParams.MatchId, goingLiveParams.MapNumber), cancellationToken);
    }

    public Task FinalizeAsync(SeriesResultParams seriesResultParams, CancellationToken cancellationToken)
    {
        return _G5Stats.SendEventAsync(new SeriesResultEvent(seriesResultParams.MatchId, new Winner((Side)(int)Utils.LoadMatchWinner(), 0), seriesResultParams.Team1SeriesScore, seriesResultParams.Team2SeriesScore, (int)seriesResultParams.TimeBeforeFreeingServerMs), cancellationToken);
    }

    public Task SendRoundStatsUpdateAsync(RoundStatusUpdateParams roundStatusUpdateParams, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
