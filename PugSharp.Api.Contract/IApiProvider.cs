namespace PugSharp.Api.Contract
{
    public interface IApiProvider
    {
        Task GoingLiveAsync(GoingLiveParams goingLiveParams, CancellationToken cancellationToken);
    }

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

        public Task GoingLiveAsync(GoingLiveParams goingLiveParams, CancellationToken cancellationToken)
        {
            return Task.WhenAll(_ApiProviders.Select(a => a.GoingLiveAsync(goingLiveParams, cancellationToken)));
        }
    }
}
