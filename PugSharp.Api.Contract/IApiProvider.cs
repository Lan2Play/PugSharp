namespace PugSharp.Api.Contract;

public interface IApiProvider
{
    Task MapVetoedAsync(MapVetoedParams mapVetoedParams, CancellationToken cancellationToken);
    Task MapPickedAsync(MapPickedParams mapPickedParams, CancellationToken cancellationToken);
    Task GoingLiveAsync(GoingLiveParams goingLiveParams, CancellationToken cancellationToken);
    Task RoundStatsUpdateAsync(RoundStatusUpdateParams roundStatusUpdateParams, CancellationToken cancellationToken);
    Task RoundMvpAsync(RoundMvpParams roundMvpParams, CancellationToken cancellationToken);
    Task FinalizeMapAsync(MapResultParams finalizeMapParams, CancellationToken cancellationToken);
    Task FinalizeAsync(SeriesResultParams seriesResultParams, CancellationToken cancellationToken);
    Task FreeServerAsync(CancellationToken cancellationToken);
}
