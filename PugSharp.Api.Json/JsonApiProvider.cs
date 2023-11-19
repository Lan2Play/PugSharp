using System.Globalization;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using PugSharp.Api.Contract;

namespace PugSharp.Api.Json;

public class JsonApiProvider : IApiProvider
{
    private readonly ILogger<JsonApiProvider> _Logger;

    private string? _ApiStatsDirectory;

    public JsonApiProvider(ILogger<JsonApiProvider> logger)
    {
        _Logger = logger;
    }

    public void Initialize(string? apiStatsDirectory)
    {
        _ApiStatsDirectory = apiStatsDirectory;
    }

    private async Task SerializeAndSaveData<T>(string fileName, T data, CancellationToken cancellationToken)
    {
        try
        {
            if (_ApiStatsDirectory == null)
            {
                return;
            }

            var fullFileName = Path.GetFullPath(Path.Combine(_ApiStatsDirectory, fileName));
            CreateStatsDirectoryIfNotExists();

            var fileStream = File.Open(fullFileName, FileMode.Create);
            await using (fileStream.ConfigureAwait(false))
            {
                await JsonSerializer.SerializeAsync(fileStream, data, cancellationToken: cancellationToken).ConfigureAwait(false);
                _Logger.LogInformation("Stored {dataType}: {fullFileName}", typeof(T), fullFileName);
            }
        }
        catch (Exception ex)
        {
            _Logger.LogError(ex, "Error storing map {dataType} to {fileName}!", typeof(T), fileName);
        }
    }

    public Task GoingLiveAsync(GoingLiveParams goingLiveParams, CancellationToken cancellationToken)
    {
        return SerializeAndSaveData($"Match_{goingLiveParams.MatchId}_golive.json", goingLiveParams, cancellationToken);
    }

    public Task FinalizeMapAsync(MapResultParams finalizeMapParams, CancellationToken cancellationToken)
    {
        return SerializeAndSaveData($"Match_{finalizeMapParams.MatchId}_mapresult.json", finalizeMapParams, cancellationToken);
    }

    public Task FinalizeAsync(SeriesResultParams seriesResultParams, CancellationToken cancellationToken)
    {
        return SerializeAndSaveData($"Match_{seriesResultParams.MatchId}_result.json", seriesResultParams, cancellationToken);
    }

    public Task FreeServerAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task RoundStatsUpdateAsync(RoundStatusUpdateParams roundStatusUpdateParams, CancellationToken cancellationToken)
    {
        var round = roundStatusUpdateParams.CurrentMap.Team1.Score + roundStatusUpdateParams.CurrentMap.Team2.Score;
        return SerializeAndSaveData(string.Create(CultureInfo.InvariantCulture, $"Match_{roundStatusUpdateParams.MatchId}_roundresult_{round}.json"), roundStatusUpdateParams, cancellationToken);
    }

    private void CreateStatsDirectoryIfNotExists()
    {
        if (_ApiStatsDirectory != null && !Directory.Exists(_ApiStatsDirectory))
        {
            Directory.CreateDirectory(_ApiStatsDirectory);
        }
    }

    public Task RoundMvpAsync(RoundMvpParams roundMvpParams, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
