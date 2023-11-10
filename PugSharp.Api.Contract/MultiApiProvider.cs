

namespace PugSharp.Api.Contract
{
    public class MultiApiProvider : IApiProvider
    {
        private readonly List<IApiProvider> _ApiProviders = new();

        public void AddApiProvider(IApiProvider apiProvider)
        {
            _ApiProviders.Add(apiProvider);
        }

        public void ClearApiProviders()
        {
            _ApiProviders.Clear();
        }

        #region IApiProvider

        public Task GoingLiveAsync(GoingLiveParams goingLiveParams, CancellationToken cancellationToken)
        {
            return Task.WhenAll(_ApiProviders.Select(a => a.GoingLiveAsync(goingLiveParams, cancellationToken)));
        }

        public Task FinalizeMapAsync(MapResultParams finalizeMapParams, CancellationToken cancellationToken)
        {
            return Task.WhenAll(_ApiProviders.Select(a => a.FinalizeMapAsync(finalizeMapParams, cancellationToken)));
        }

        public Task FinalizeAsync(SeriesResultParams seriesResultParams, CancellationToken cancellationToken)
        {
            return Task.WhenAll(_ApiProviders.Select(a => a.FinalizeAsync(seriesResultParams, cancellationToken)));
        }

        public Task RoundStatsUpdateAsync(RoundStatusUpdateParams roundStatusUpdateParams, CancellationToken cancellationToken)
        {
            return Task.WhenAll(_ApiProviders.Select(a => a.RoundStatsUpdateAsync(roundStatusUpdateParams, cancellationToken)));
        }

        public IReadOnlyList<ProviderCommand> LoadProviderCommands()
        {
            return _ApiProviders.SelectMany(x => x.LoadProviderCommands()).ToList();
        }

        #endregion
    }
}
