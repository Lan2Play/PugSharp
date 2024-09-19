using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;

namespace PugSharp.Api.G5Api;

public sealed class G5ApiClient
{
    private readonly ILogger<G5ApiClient> _Logger;

    private readonly HttpClient _HttpClient;

    private string? _ApiUrl;
    private string? _ApiHeader;
    private string? _ApiHeadeValue;

    public G5ApiClient(HttpClient httpClient, ILogger<G5ApiClient> logger)
    {
        _HttpClient = httpClient;
        _Logger = logger;
    }

    public void Initialize(string g5ApiUrl, string g5ApiHeader, string g5ApiHeaderValue)
    {
        _Logger.LogInformation("Initialize G5Api with BaseUrl: {Url}", g5ApiUrl);

        var modifiedApiUrl = g5ApiUrl;

        if (!g5ApiUrl.EndsWith("v2", StringComparison.OrdinalIgnoreCase) && !g5ApiUrl.EndsWith("v2/", StringComparison.OrdinalIgnoreCase))
        {
            modifiedApiUrl = $"{g5ApiUrl.TrimEnd('/')}/v2";
        }

        _ApiUrl = modifiedApiUrl;
        _ApiHeader = g5ApiHeader;
        _ApiHeadeValue = g5ApiHeaderValue;
    }

    public async Task<bool> SendEventAsync(EventBase eventToSend, CancellationToken cancellationToken)
    {
        try
        {
            using var jsonContent = new StringContent(
                JsonSerializer.Serialize(eventToSend, eventToSend.GetType()),
                Encoding.UTF8,
                "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, _ApiUrl)
            {
                Content = jsonContent,
            };

            if (!string.IsNullOrEmpty(_ApiHeader))
            {
                httpRequest.Headers.Add(_ApiHeader, _ApiHeadeValue);
            }

            using var httpResponseMessage = await _HttpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

            if (httpResponseMessage == null)
            {
                _Logger.LogError("G5 API request {Event} failed. No HTTP status code available.", eventToSend.EventName);
                return false;
            }

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                _Logger.LogInformation("G5 API request {Event} was successful, HTTP status code = {StatusCode}", eventToSend.EventName, httpResponseMessage.StatusCode);
                return true;
            }

            _Logger.LogError("G5 API request {Event} failed. HTTP status code = {StatusCode} content: {Content}", eventToSend.EventName, httpResponseMessage.StatusCode, httpResponseMessage.Content.ToString());

            return false;
        }
        catch (Exception ex)
        {
            _Logger.LogError(ex, "Error sending event to G5 API. EventName {EventName}", eventToSend.EventName);
        }

        return false;
    }
}
