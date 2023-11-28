using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace PugSharp.ApiStats;

public class BaseApi
{
    private readonly ILogger<BaseApi> _Logger;

    protected readonly HttpClient HttpClient;

    protected BaseApi(HttpClient httpClient, ILogger<BaseApi> logger)
    {
        HttpClient = httpClient;
        _Logger = logger;
    }

    protected void InitializeBase(string? baseUrl, string? authKey)
    {
        if (string.IsNullOrEmpty(baseUrl))
        {
            return;
        }

        try
        {
            if (!baseUrl.EndsWith('/'))
            {
                baseUrl += "/";
            }

            _Logger.LogInformation("Using BaseURL : \"{url}\" and authKey \"{authKey}\"", baseUrl, authKey);

            HttpClient.BaseAddress = new Uri(baseUrl);

            HttpClient.DefaultRequestHeaders.Remove(HeaderNames.Authorization);

            if (!string.IsNullOrEmpty(authKey))
            {
                HttpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, authKey);
            }
        }
        catch (Exception ex)
        {
            _Logger.LogError(ex, "Error initializing {type} some api features may not work correctly!", GetType().Name);
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
                _Logger.LogInformation("API request was successful, HTTP status code = {statusCode}", httpResponseMessage.StatusCode);

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
}
