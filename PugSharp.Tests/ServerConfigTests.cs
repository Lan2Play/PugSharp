using System.Text.Json;

using PugSharp.Config;

namespace PugSharp.Tests;

public class ServerConfigTests
{
    [Fact]
    public void ServerConfigCanBeSerializedAndDeserialized()
    {
        var originalConfig = new ServerConfig
        {
            Locale = "en",
            AllowPlayersWithoutMatch = true,
            AutoloadConfig = "https://example.com/config.json",
            AutoloadConfigAuthToken = "test-token"
        };

        var json = JsonSerializer.Serialize(originalConfig);
        var deserializedConfig = JsonSerializer.Deserialize<ServerConfig>(json);

        Assert.NotNull(deserializedConfig);
        Assert.Equal(originalConfig.Locale, deserializedConfig.Locale);
        Assert.Equal(originalConfig.AllowPlayersWithoutMatch, deserializedConfig.AllowPlayersWithoutMatch);
        Assert.Equal(originalConfig.AutoloadConfig, deserializedConfig.AutoloadConfig);
        Assert.Equal(originalConfig.AutoloadConfigAuthToken, deserializedConfig.AutoloadConfigAuthToken);
    }

    [Fact]
    public void ServerConfigJsonPropertyNamesAreCorrect()
    {
        var config = new ServerConfig
        {
            AutoloadConfig = "test-config",
            AutoloadConfigAuthToken = "test-token"
        };

        var json = JsonSerializer.Serialize(config);
        
        Assert.Contains("\"autoload_config\"", json, StringComparison.Ordinal);
        Assert.Contains("\"autoload_config_auth_token\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void ServerConfigDefaultsToEmptyAutoloadConfig()
    {
        var serverConfig = new ServerConfig();

        Assert.Equal(string.Empty, serverConfig.AutoloadConfig);
        Assert.Equal(string.Empty, serverConfig.AutoloadConfigAuthToken);
    }
}