using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace PugSharp.ApiStats;

public class BaseApi : IDisposable
{
    private readonly ILogger<BaseApi> _Logger;

    private bool _DisposedValue;

    protected HttpClient? HttpClient { get; private set; }

    protected BaseApi(ILogger<BaseApi> logger)
    {
        _Logger = logger;
    }

    protected void InitializeBase(string? baseUrl, string? authKey)
    {
        if (string.IsNullOrEmpty(baseUrl))
        {
            return;
        }

        if (!baseUrl.EndsWith('/'))
        {
            baseUrl += "/";
        }

        _Logger.LogInformation("Using BaseURL : \"{url}\" and authKey \"{authKey}\"", baseUrl, authKey);
        HttpClient = new HttpClient()
        {
            BaseAddress = new Uri(baseUrl),
        };

        if (!string.IsNullOrEmpty(authKey))
        {
            HttpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, authKey);
        }
    }

    protected async Task HandleResponseAsync(HttpResponseMessage? httpResponseMessage, CancellationToken cancellationToken)
    {
        if (httpResponseMessage == null)
        {
            return;
        }

        try
        {
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                _Logger.LogInformation("API request was succesful, HTTP status code = {statusCode}", httpResponseMessage.StatusCode);

                var responseContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                _Logger.LogInformation("ResponseContent: {responseContent}", responseContent);
            }
            else
            {
                _Logger.LogError("API request failed, HTTP status code = {statusCode}", httpResponseMessage.StatusCode);

                var responseContent = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                _Logger.LogError("ResponseContent: {responseContent}", responseContent);
            }
        }
        catch (Exception e)
        {
            _Logger.LogError(e, "Error handling response");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_DisposedValue)
        {
            if (disposing)
            {
                HttpClient?.Dispose();
            }

            _DisposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
