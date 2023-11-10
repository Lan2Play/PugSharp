
namespace PugSharp.Api.Contract
{
    public interface IApiProvider
    {
        Task GoingLiveAsync(GoingLiveParams goingLiveParams, CancellationToken cancellationToken);
        Task FinalizeMapAsync(MapResultParams finalizeMapParams, CancellationToken cancellationToken);
        Task FinalizeAsync(SeriesResultParams seriesResultParams, CancellationToken cancellationToken);
        Task RoundStatsUpdateAsync(RoundStatusUpdateParams roundStatusUpdateParams, CancellationToken cancellationToken);

    }
}
