using Microsoft.Extensions.Logging;

namespace PugSharp.ApiStats;

public class DemoUploader : BaseApi
{
    private readonly ILogger<DemoUploader> _Logger;

    public DemoUploader(ILogger<DemoUploader> logger) : base(logger)
    {
        _Logger = logger;
    }

    public void Initialize(string demoUploadUrl, string demoUploadKey)
    {
        _Logger.LogInformation("Initialize Api Stats with BaseUrl: {url}", demoUploadUrl);
        InitializeBase(demoUploadUrl, demoUploadKey);
    }

    public async Task UploadDemoAsync(string? demoFile, CancellationToken cancellationToken)
    {
        if (HttpClient == null || demoFile == null)
        {
            return;
        }

        try
        {
            _Logger.LogInformation("Upload Demo Async!");

            var demoFileStream = File.OpenRead(demoFile);
            await using (demoFileStream.ConfigureAwait(false))
            {
                using var fileStreamContent = new StreamContent(demoFileStream);
                var request = new HttpRequestMessage(HttpMethod.Post, string.Empty)
                {
                    Content = fileStreamContent,
                };

                request.Headers.Add("PugSharp-DemoName", Path.GetFileName(demoFile));

                var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                await HandleResponseAsync(response, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _Logger.LogError(ex, "Error uploading demo!");
        }
    }
}
