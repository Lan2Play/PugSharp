using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;

using Microsoft.Extensions.DependencyInjection;

using PugSharp.Api.G5Api.Tests.Models;

using Testcontainers.MariaDb;

using Xunit;

namespace PugSharp.Api.G5Api.Tests.Fixtures;
public class G5ApiFixture : IAsyncLifetime
{
    private const string _ManagementKey = "yourStrong(!)ManagementKey";
    private const string _DatabaseHost = "database";
    private const string _ApiHost = "g5api";
    private const ushort _ApiPort = 8080;

    private readonly HttpClient _Http = new();

    private readonly INetwork _Network;
    private readonly MariaDbContainer _DatabaseContainer;
    private readonly IContainer _ApiContainer;
    private readonly MemoryStream _DatabaseContainerStdOut = new();
    private readonly MemoryStream _DatabaseContainerStdErr = new();
    private readonly MemoryStream _ApiContainerStdOut = new();
    private readonly MemoryStream _ApiContainerStdErr = new();

    public string PublicApiUrl => $"http://{_ApiContainer.Hostname}:{_ApiContainer.GetMappedPublicPort(_ApiPort)}";

    public G5ApiFixture()
    {
        _Network = new NetworkBuilder()
            //.WithDriver(DotNet.Testcontainers.Configurations.NetworkDriver.Host)
            .Build();

        _DatabaseContainer = new MariaDbBuilder()
            .WithNetwork(_Network)
            .WithNetworkAliases(_DatabaseHost)
            .WithDatabase("get5")
            .WithOutputConsumer(
                Consume.RedirectStdoutAndStderrToStream(_DatabaseContainerStdOut, _DatabaseContainerStdErr)
            )
            .Build();

        _ApiContainer = new ContainerBuilder()
            .WithImage("ghcr.io/phlexplexico/g5api:latest")
            .WithImagePullPolicy(PullPolicy.Missing)
            .WithNetwork(_Network)
            .WithNetworkAliases(_ApiHost)
            .WithEnvironment("NODE_ENV", "production")
            .WithEnvironment("PORT", _ApiPort.ToString(CultureInfo.InvariantCulture))
            .WithEnvironment("DBKEY", "de84096947c7860ea6c1479573492f23")
            .WithEnvironment("STEAMAPIKEY", "")
            .WithEnvironment("HOSTNAME", "http://192.168.178.52")
            .WithEnvironment("SHAREDSECRET", $"{_ManagementKey}")
            .WithEnvironment("CLIENTHOME", $"http://192.168.178.52")
            .WithEnvironment("APIURL", $"http://192.168.178.52")
            .WithEnvironment("SQLUSER", $"{MariaDbBuilder.DefaultUsername}")
            .WithEnvironment("SQLPASSWORD", $"{MariaDbBuilder.DefaultPassword}")
            .WithEnvironment("SQLPORT", $"{MariaDbBuilder.MariaDbPort}")
            .WithEnvironment("DATABASE", "get5")
            .WithEnvironment("SQLHOST", _DatabaseHost)
            .WithEnvironment("ADMINS", "")
            .WithEnvironment("SUPERADMINS", "")
            .WithEnvironment("REDISURL", "")
            .WithEnvironment("REDISTTL", "0")
            .WithEnvironment("USEREDIS", "false")
            .WithEnvironment("UPLOADDEMOS", "true")
            .WithEnvironment("LOCALLOGINS", "true")
            .WithPortBinding(_ApiPort, assignRandomHostPort: true)
            // Wait until the API is launched, has performed migrations, and is ready to accept requests
            .WithWaitStrategy(Wait
                .ForUnixContainer()
                .UntilHttpRequestIsSucceeded(r => r
                    .ForPath("/")
                    .ForPort(_ApiPort)
                    .ForStatusCode(HttpStatusCode.OK)
                )
            )
            .WithOutputConsumer(
                Consume.RedirectStdoutAndStderrToStream(_ApiContainerStdOut, _ApiContainerStdErr)
            )
            .DependsOn(_DatabaseContainer)
            .Build();
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Introduce a timeout to avoid waiting forever for the containers to start
            // in case something goes wrong (e.g. wait strategy never succeeds).
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

            await _Network.CreateAsync(timeoutCts.Token).ConfigureAwait(false);
            await _DatabaseContainer.StartAsync(timeoutCts.Token).ConfigureAwait(false);
            await _ApiContainer.StartAsync(timeoutCts.Token).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is OperationCanceledException or TimeoutException)
        {
            throw new OperationCanceledException(
                "Failed to start the containers within the allotted timeout. " +
                "This probably means that something went wrong during container initialization. " +
                "See the logs for more info." +
                Environment.NewLine + Environment.NewLine +
                GetLogs(),
                ex
            );
        }
    }

    public string GetLogs()
    {
        var databaseContainerStdOutText = Encoding.UTF8.GetString(
            _DatabaseContainerStdOut.ToArray()
        );

        var databaseContainerStdErrText = Encoding.UTF8.GetString(
            _DatabaseContainerStdErr.ToArray()
        );

        var apiContainerStdOutText = Encoding.UTF8.GetString(
            _ApiContainerStdOut.ToArray()
        );

        var apiContainerStdErrText = Encoding.UTF8.GetString(
            _ApiContainerStdErr.ToArray()
        );

        // API logs are typically more relevant, so put them first
        return
            $"""
             # API container STDOUT:

             {apiContainerStdOutText}

             # API container STDERR:

             {apiContainerStdErrText}
             
             # Database container STDOUT:
             
             {databaseContainerStdOutText}
             
             # Database container STDERR:
             
             {databaseContainerStdErrText}
             """;
    }

    public async Task DisposeAsync()
    {
        await _ApiContainer.DisposeAsync().ConfigureAwait(false);
        await _DatabaseContainer.DisposeAsync().ConfigureAwait(false);
        await _Network.DisposeAsync().ConfigureAwait(false);

        await _DatabaseContainerStdOut.DisposeAsync().ConfigureAwait(false);
        await _DatabaseContainerStdErr.DisposeAsync().ConfigureAwait(false);
        await _ApiContainerStdOut.DisposeAsync().ConfigureAwait(false);
        await _ApiContainerStdErr.DisposeAsync().ConfigureAwait(false);

        _Http.Dispose();
    }

    public async Task<G5ApiClient> CreateClientAsync()
    {
        var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(3) };

        var registerHttpRequest = new HttpRequestMessage(HttpMethod.Post, $"{PublicApiUrl}/register?username=test&password=supersecure")
        {
            Content = JsonContent.Create(new Register() { SteamId = "76561198025644194" }),
        };

        using var registerResponseMessage = await httpClient.SendAsync(registerHttpRequest, CancellationToken.None).ConfigureAwait(true);

        var registerResponseText = await registerResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(true);
        Assert.NotNull(registerResponseText);

        var loginHttpRequest = new HttpRequestMessage(HttpMethod.Post, $"{PublicApiUrl}/login?username=test&password=supersecure")
        {
            Content = new StringContent(string.Empty),
        };

        using var loginResponseMessage = await httpClient.SendAsync(loginHttpRequest, CancellationToken.None).ConfigureAwait(true);

        var loginResponseText = await loginResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(true);
        Assert.NotNull(loginResponseText);

        var createServerRequest = new HttpRequestMessage(HttpMethod.Post, $"{PublicApiUrl}/servers")
        {
            Content = JsonContent.Create(new List<Server>()
            {
                new()
                {
                    IpString = "192.168.178.52",
                    Port = 27015,
                    DisplayName = "Phlex's Temp Server",
                    RconPassword = "password",
                    PublicServer = true,
                },
            }),
        };

        using var createServerResponseMessage = await httpClient.SendAsync(createServerRequest, CancellationToken.None).ConfigureAwait(true);

        var getServersRequest = new HttpRequestMessage(HttpMethod.Get, $"{PublicApiUrl}/servers");

        using var getServersHttpResponseMessage = await httpClient.SendAsync(getServersRequest, CancellationToken.None).ConfigureAwait(true);

        var getServersResponseText = await getServersHttpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(true);
        Assert.NotNull(getServersResponseText);

        // Create teams
        await CreateTeams(httpClient).ConfigureAwait(false);

        // Create match
        var match = await CreateMatch(httpClient).ConfigureAwait(false);

        return CreateApiClient(match);
    }

    private G5ApiClient CreateApiClient(MatchResponseMatch match)
    {
        var services = new ServiceCollection();

        services.AddHttpClient<G5ApiClient>();

        services.AddSingleton<G5ApiClient>();

        var sp = services.BuildServiceProvider();

        var apiClient = sp.GetRequiredService<G5ApiClient>();

        apiClient.Initialize(PublicApiUrl, "Authorization", match.ApiKey);
        return apiClient;
    }

    private async Task<MatchResponseMatch> CreateMatch(HttpClient httpClient)
    {
        var createMatchRequest = new HttpRequestMessage(HttpMethod.Post, $"{PublicApiUrl}/matches")
        {
            Content = JsonContent.Create(new List<Match>()
            {
                new()
                {
                    ServerId = 1,
                    Team1Id = 1,
                    Team2Id = 2,
                    IgnoreServer = true,
                    MaxMaps = 1,
                    Title = "Drecksmatch",
                    SkipVeto = false,
                    VetoMapPool = "de_dust2, de_cache, de_mirage",
                },
            }),
        };

        using var createMatchResponseMessage = await httpClient.SendAsync(createMatchRequest, CancellationToken.None).ConfigureAwait(true);

        var getMatchRequest = new HttpRequestMessage(HttpMethod.Get, $"{PublicApiUrl}/matches/1");

        using var getMatchHttpResponseMessage = await httpClient.SendAsync(getMatchRequest, CancellationToken.None).ConfigureAwait(true);

        var getMatchResponse = await getMatchHttpResponseMessage.Content.ReadFromJsonAsync<MatchResponse>().ConfigureAwait(true);

        Assert.NotNull(getMatchResponse);

        return getMatchResponse.Match;
    }

    private async Task CreateTeams(HttpClient httpClient)
    {
        var createTeam1Request = new HttpRequestMessage(HttpMethod.Post, $"{PublicApiUrl}/teams")
        {
            Content = JsonContent.Create(new List<Team>()
            {
                new()
                {
                    Name = "Kraddlers",
                    Flag = "",
                    Logo = "",
                    Tag = "",
                    IsPublic = false,
                    Players = new Dictionary<string, PlayerAuth>(StringComparer.OrdinalIgnoreCase)
                    {
                        {"76561198025644200", new PlayerAuth(){ Name = "1", IsCaptain = false, IsCoach = false } },
                        {"76561198025644194", new PlayerAuth(){ Name = "2", IsCaptain = false, IsCoach = false } },
                    },
                },
            }),
        };

        using var createTeam1ResponseMessage = await httpClient.SendAsync(createTeam1Request, CancellationToken.None).ConfigureAwait(true);

        var createTeam2Request = new HttpRequestMessage(HttpMethod.Post, $"{PublicApiUrl}/teams")
        {
            Content = JsonContent.Create(new List<Team>()
            {
                new()
                {
                    Name = "Knaddles",
                    Flag = "",
                    Logo = "",
                    Tag = "",
                    IsPublic = false,
                    Players = new Dictionary<string, PlayerAuth>(StringComparer.OrdinalIgnoreCase)
                    {
                        {"76561198025644201", new PlayerAuth(){ Name = "3", IsCaptain = false, IsCoach = false } },
                        {"76561198025644195", new PlayerAuth(){ Name = "4", IsCaptain = false, IsCoach = false } },
                    },
                },
            }),
        };

        using var createTeam2ResponseMessage = await httpClient.SendAsync(createTeam2Request, CancellationToken.None).ConfigureAwait(true);
    }
}
