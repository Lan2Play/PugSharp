using Microsoft.Extensions.Logging;
using PugSharp.Logging;

namespace PugSharp.ApiStats
{
    public class DemoUploader : BaseApi
    {
        private static readonly ILogger<ApiStats> _Logger = LogManager.CreateLogger<ApiStats>();

        public DemoUploader(string demoUploadUrl, string demoUploadKey) : base(demoUploadUrl, demoUploadKey)
        {
            _Logger.LogInformation("Create Api Stats with BaseUrl: {url}", demoUploadUrl);
        }

        public async Task UploadDemoAsync(string demoFile, CancellationToken cancellationToken)
        {
            if (HttpClient == null)
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
                    using var formData = new MultipartFormDataContent
                    {
                        fileStreamContent,
                    };

                    var request = new HttpRequestMessage(HttpMethod.Post, string.Empty)
                    {
                        Content = formData,
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
}
