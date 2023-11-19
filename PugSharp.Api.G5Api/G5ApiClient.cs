using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace PugSharp.Api.G5Api;

public sealed class G5ApiClient
{
    private readonly ILogger<G5ApiClient> _Logger;

    private readonly HttpClient _HttpClient;

    private string? _ApiUrl;
    private string? _ApiHeader;
    private string? _ApiHeadeValue;

    public G5ApiClient(HttpClient httpClient,ILogger<G5ApiClient> logger)
    {
        _HttpClient = httpClient;
        _Logger = logger;
    }

    public void Initialize(string g5ApiUrl, string g5ApiHeader, string g5ApiHeaderValue)
    {
        _Logger.LogInformation("Initialize G5Api with BaseUrl: {url}", g5ApiUrl);
        _ApiUrl = g5ApiUrl;
        _ApiHeader = g5ApiHeader;
        _ApiHeadeValue = g5ApiHeaderValue;
    }

    public void UpdateConfig(string g5ApiUrl, string g5ApiHeader, string g5ApiHeaderValue)
    {
        _ApiUrl = g5ApiUrl;
        _ApiHeader = g5ApiHeader;
        _ApiHeadeValue = g5ApiHeaderValue;
    }

    public async Task SendEventAsync(EventBase eventToSend, CancellationToken cancellationToken)
    {
        try
        {
            using var jsonContent = new StringContent(
                JsonSerializer.Serialize(eventToSend),
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
                return;
            }

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                _Logger.LogInformation("G5 API request was succesful, HTTP status code = {statusCode}", httpResponseMessage.StatusCode);
            }
            else
            {
                _Logger.LogError("G5 API request failed, HTTP status code = {statusCode}", httpResponseMessage.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _Logger.LogError(ex, "Error sending event to G5 API. EventName {EventName}", eventToSend.EventName);
        }
    }
}
