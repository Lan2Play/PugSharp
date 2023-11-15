using PugSharp.Api.Contract;
using PugSharp.Api.G5Api;
using PugSharp.Server.Contract;
using System.Globalization;

namespace PugSharp;

public partial class G5ApiProvider : IApiProvider
{
    private readonly G5ApiClient _G5Stats;
    private readonly ICsServer _CsServer;

    public G5ApiProvider(G5ApiClient apiClient, ICsServer csServer)
    {
        _G5Stats = apiClient;
        _CsServer = csServer;
    }

    #region IApiProvider

    public Task FinalizeMapAsync(MapResultParams finalizeMapParams, CancellationToken cancellationToken)
    {
        var (CtScore, TScore) = _CsServer.LoadTeamsScore();
        var emptyStatsTeam = new StatsTeam(string.Empty, string.Empty, 0, 0, 0, 0, Enumerable.Empty<StatsPlayer>());

        var mapResultEvent = new MapResultEvent
        {
            MatchId = finalizeMapParams.MatchId,
            MapNumber = finalizeMapParams.MapNumber,
            Winner = new Winner(CtScore > TScore ? Side.CT : Side.T, finalizeMapParams.Team1Score > finalizeMapParams.Team2Score ? 1 : 2),
            StatsTeam1 = emptyStatsTeam,
            StatsTeam2 = emptyStatsTeam,
        };

        return _G5Stats.SendEventAsync(mapResultEvent, cancellationToken);
    }

    public Task GoingLiveAsync(GoingLiveParams goingLiveParams, CancellationToken cancellationToken)
    {
        var goingLiveEvent = new GoingLiveEvent
        {
            MatchId = goingLiveParams.MatchId,
            MapNumber = goingLiveParams.MapNumber,
        };
        return _G5Stats.SendEventAsync(goingLiveEvent, cancellationToken);
    }

    public Task FinalizeAsync(SeriesResultParams seriesResultParams, CancellationToken cancellationToken)
    {
        var (CtScore, TScore) = _CsServer.LoadTeamsScore();
        var seriesResultEvent = new SeriesResultEvent()
        {
            MatchId = seriesResultParams.MatchId,
            Winner = new Winner(CtScore > TScore ? Side.CT : Side.T, seriesResultParams.Team1SeriesScore > seriesResultParams.Team2SeriesScore ? 1 : 2),
            Team1SeriesScore = seriesResultParams.Team1SeriesScore,
            Team2SeriesScore = seriesResultParams.Team2SeriesScore,
            TimeUntilRestore = (int)seriesResultParams.TimeBeforeFreeingServerMs,
        };
        return _G5Stats.SendEventAsync(seriesResultEvent, cancellationToken);
    }

    public Task FreeServerAsync(CancellationToken cancellationToken)
    {
        // Not Required. Handled via TimeUntilRestore of in the Finalize event
        return Task.CompletedTask;
    }

    public Task RoundStatsUpdateAsync(RoundStatusUpdateParams roundStatusUpdateParams, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task RoundMvpAsync(RoundMvpParams roundMvpParams, CancellationToken cancellationToken)
    {
        var player = roundMvpParams.Player;

        var roundMvpEvent = new RoundMvpEvent
        {
            MapNumber = roundMvpParams.MapNumber,
            MatchId = roundMvpParams.MatchId,
            Reason = roundMvpParams.Reason,
            RoundNumber = roundMvpParams.RoundNumber,
            Player = new Player(player.SteamId.ToString(CultureInfo.InvariantCulture), player.Name, player.UserId, (Side)player.Side, player.IsBot)
        };

        return _G5Stats.SendEventAsync(roundMvpEvent, cancellationToken);
    }


    #endregion

    #region Get5Api Commands


    #endregion
}
