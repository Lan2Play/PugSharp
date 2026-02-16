using System.Globalization;

using PugSharp.Api.Contract;
using PugSharp.Api.G5Api;
using PugSharp.Server.Contract;

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

    public Task MapVetoedAsync(MapVetoedParams mapVetoedParams, CancellationToken cancellationToken)
    {
        var mapVetoedEvent = new MapVetoedEvent
        {
            MatchId = mapVetoedParams.MatchId,
            MapName = mapVetoedParams.MapName,
            Team = mapVetoedParams.Team,
        };

        return _G5Stats.SendEventAsync(mapVetoedEvent, cancellationToken);
    }

    public Task MapPickedAsync(MapPickedParams mapPickedParams, CancellationToken cancellationToken)
    {
        var mapPickedEvent = new MapPickedEvent
        {
            MatchId = mapPickedParams.MatchId,
            MapName = mapPickedParams.MapName,
            MapNumber = mapPickedParams.MapNumber,
            Team = mapPickedParams.Team,
        };

        return _G5Stats.SendEventAsync(mapPickedEvent, cancellationToken);
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

    public async Task RoundStatsUpdateAsync(RoundStatusUpdateParams roundStatusUpdateParams, CancellationToken cancellationToken)
    {
        var team1Won = roundStatusUpdateParams.CurrentMap.WinnerTeamName.Equals(roundStatusUpdateParams.TeamInfo1.TeamName, StringComparison.OrdinalIgnoreCase);

        var winner = new Winner(roundStatusUpdateParams.CurrentMap.WinnerTeamSide == TeamSide.T ? Side.T : Side.CT, team1Won ? 1 : 2);

        var roundNumber = roundStatusUpdateParams.CurrentMap.Team1.Score + roundStatusUpdateParams.CurrentMap.Team2.Score;
        var roundEndEvent = new RoundEndedEvent
        {
            MatchId = roundStatusUpdateParams.MatchId,
            MapNumber = roundStatusUpdateParams.MapNumber,
            RoundNumber = roundNumber,
            Reason = roundStatusUpdateParams.Reason,
            RoundTime = roundStatusUpdateParams.RoundTime,
            Winner = winner,
            StatsTeam1 = new StatsTeam(roundStatusUpdateParams.TeamInfo1.TeamId, roundStatusUpdateParams.TeamInfo1.TeamName, 0, roundStatusUpdateParams.CurrentMap.Team1.Score, roundStatusUpdateParams.CurrentMap.Team1.ScoreCT, roundStatusUpdateParams.CurrentMap.Team1.ScoreT, roundStatusUpdateParams.CurrentMap.Team1.Players.Select(p => CreateStatsPlayer(p.Key, p.Value)).ToList()),
            StatsTeam2 = new StatsTeam(roundStatusUpdateParams.TeamInfo2.TeamId, roundStatusUpdateParams.TeamInfo2.TeamName, 0, roundStatusUpdateParams.CurrentMap.Team2.Score, roundStatusUpdateParams.CurrentMap.Team2.ScoreCT, roundStatusUpdateParams.CurrentMap.Team2.ScoreT, roundStatusUpdateParams.CurrentMap.Team2.Players.Select(p => CreateStatsPlayer(p.Key, p.Value)).ToList()),
        };

        await _G5Stats.SendEventAsync(roundEndEvent, cancellationToken).ConfigureAwait(false);

        var roundStatsUpdateEvent = new RoundStatsUpdatedEvent
        {
            MatchId = roundStatusUpdateParams.MatchId,
            MapNumber = roundStatusUpdateParams.MapNumber,
            RoundNumber = roundNumber,
        };

        await _G5Stats.SendEventAsync(roundStatsUpdateEvent, cancellationToken).ConfigureAwait(false);
    }

    private static StatsPlayer CreateStatsPlayer(string steamId, IPlayerStatistics playerStatistics)
    {
        return new StatsPlayer
        {
            SteamId = steamId,
            Name = playerStatistics.Name,
            Stats = new PlayerStats
            {
                Assists = playerStatistics.Assists,
                BombDefuses = playerStatistics.BombDefuses,
                BombPlants = playerStatistics.BombPlants,
                Damage = playerStatistics.Damage,
                Deaths = playerStatistics.Deaths,
                EnemiesFlashed = playerStatistics.EnemiesFlashed,
                FirstDeathsCT = playerStatistics.FirstDeathCt,
                FirstDeathsT = playerStatistics.FirstDeathT,
                FirstKillsCT = playerStatistics.FirstKillCt,
                FirstKillsT = playerStatistics.FirstKillT,
                FlashAssists = playerStatistics.FlashbangAssists,
                FriendliesFlashed = playerStatistics.FriendliesFlashed,
                HeadshotKills = playerStatistics.HeadshotKills,
                Kast = playerStatistics.Kast,
                Kills = playerStatistics.Kills,
                Kills1 = playerStatistics.Count1K,
                Kills2 = playerStatistics.Count2K,
                Kills3 = playerStatistics.Count3K,
                Kills4 = playerStatistics.Count4K,
                Kills5 = playerStatistics.Count5K,
                KnifeKills = playerStatistics.KnifeKills,
                Mvps = playerStatistics.Mvp,
                OneV1s = playerStatistics.V1,
                OneV2s = playerStatistics.V2,
                OneV3s = playerStatistics.V3,
                OneV4s = playerStatistics.V4,
                OneV5s = playerStatistics.V5,
                RoundsPlayed = playerStatistics.RoundsPlayed,
                Score = playerStatistics.ContributionScore,
                Suicides = playerStatistics.Suicides,
                TeamKills = playerStatistics.TeamKills,
                TradeKills = playerStatistics.TradeKill,
                UtilityDamage = playerStatistics.UtilityDamage,
            },
        };
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
            Player = new Player(player.SteamId.ToString(CultureInfo.InvariantCulture), player.Name, player.UserId, (Side)player.Side, player.IsBot),
        };

        return _G5Stats.SendEventAsync(roundMvpEvent, cancellationToken);
    }

    public Task FinalizeMapAsync(MapResultParams finalizeMapParams, CancellationToken cancellationToken)
    {
        var (CtScore, TScore) = _CsServer.LoadTeamsScore();
        var emptyStatsTeam = new StatsTeam(string.Empty, string.Empty, 0, 0, 0, 0, []);

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

    public Task SendKnifeRoundStartedAsync(KnifeRoundStartedParams knifeRoundStartedParams, CancellationToken cancellationToken)
    {
        var knifeRoundStartedEvent = new KnifeRoundStartedEvent
        {
            MatchId = knifeRoundStartedParams.MatchId,
            MapNumber = knifeRoundStartedParams.MapNumber
        };
        return _G5Stats.SendEventAsync(knifeRoundStartedEvent, cancellationToken);
    }

    public Task SendKnifeRoundWonAsync(KnifeRoundWonParams knifeRoundWonParams, CancellationToken cancellationToken)
    {
        var knifeRoundWonEvent = new KnifeRoundWonEvent
        {
            MatchId = knifeRoundWonParams.MatchId,
            MapNumber = knifeRoundWonParams.MapNumber,
            TeamNumber = 1, // Assuming team 1 won; this would need to be calculated based on the winning side
            Side = knifeRoundWonParams.WinningSide,
            Swapped = knifeRoundWonParams.Swapped
        };
        return _G5Stats.SendEventAsync(knifeRoundWonEvent, cancellationToken);
    }

    #endregion
}
