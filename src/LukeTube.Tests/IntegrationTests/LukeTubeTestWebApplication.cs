using System.Net.Http.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using JetBrains.Annotations;
using LukeTubeLib.Models.Pushshift;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LukeTube.Tests.IntegrationTests;

[UsedImplicitly]
public sealed class LukeTubeTestWebApplication : IAsyncLifetime
{
    private readonly IDockerContainer _postgreSqlContainer;
    private readonly IDockerContainer _redisContainer;
    private readonly IDockerNetwork _lukeTubeNetwork;
    private static string _postgreSqlConnectionString;
    private static string _redisConnectionString;

    private static readonly LukeTubeWorkerImage LukeTubeWorkerImage = new();
    private readonly IDockerContainer _lukeTubeWorkerContainer;

    public LukeTubeTestWebApplication()
    {
        _lukeTubeNetwork = new TestcontainersNetworkBuilder()
            .WithName(Guid.NewGuid().ToString("D"))
            .Build();

        const string lukeTubeDb = "lukeTubeDb";
        var postgreSqlConfiguration = new PostgreSqlTestcontainerConfiguration();
        postgreSqlConfiguration.Username = "postgres";
        postgreSqlConfiguration.Password = "postgres";
        postgreSqlConfiguration.Database = "SubredditDb";
        _postgreSqlConnectionString = $"host={lukeTubeDb};database={postgreSqlConfiguration.Database};username={postgreSqlConfiguration.Username};password={postgreSqlConfiguration.Password};";

        _postgreSqlContainer = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithHostname("db")
            .WithDatabase(postgreSqlConfiguration)
            .WithNetwork(_lukeTubeNetwork)
            .WithPortBinding(5432, 5432)
            .WithExposedPort(5432)
            .WithHostname(lukeTubeDb)
            .Build();

        var redisConfiguration = new RedisTestcontainerConfiguration();
        _redisContainer = new TestcontainersBuilder<RedisTestcontainer>()
            .WithDatabase(redisConfiguration)
            .WithNetwork(_lukeTubeNetwork)
            .WithPortBinding(6379, 6379)
            .WithExposedPort(6379)
            .Build();
        _redisConnectionString = $"{_redisContainer.Hostname},abortConnect=False";

        _lukeTubeWorkerContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage(LukeTubeWorkerImage)
            .WithNetwork(_lukeTubeNetwork)
            .WithEnvironment("CONNECTION_STRINGS__POSTGRESQL", _postgreSqlConnectionString)
            // .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole()) // enable to send logs to console
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _lukeTubeNetwork.CreateAsync()
            .ConfigureAwait(false);

        await LukeTubeWorkerImage.InitializeAsync()
            .ConfigureAwait(false);

        await _postgreSqlContainer.StartAsync()
            .ConfigureAwait(false);

        await _redisContainer.StartAsync()
            .ConfigureAwait(false);

        await _lukeTubeWorkerContainer.StartAsync()
            .ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync()
            .ConfigureAwait(false);

        await _redisContainer.DisposeAsync()
            .ConfigureAwait(false);

        await LukeTubeWorkerImage.DisposeAsync()
            .ConfigureAwait(false);

        await _lukeTubeWorkerContainer.DisposeAsync()
            .ConfigureAwait(false);

        await _lukeTubeNetwork.DeleteAsync()
            .ConfigureAwait(false);
    }

    public sealed class Api : IClassFixture<LukeTubeTestWebApplication>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _webApplicationFactory;

        private readonly IServiceScope _serviceScope;

        private readonly HttpClient _httpClient;
        private string postgreSqlConnectionString = $"host=localhost;database=SubredditDb;username=postgres;password=postgres";

        public Api(LukeTubeTestWebApplication lukeTubeTestWebApplication)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "http://+:82");
            Environment.SetEnvironmentVariable("CONNECTION_STRINGS__POSTGRESQL", postgreSqlConnectionString);
            Environment.SetEnvironmentVariable("CONNECTION_STRINGS__REDIS", "localhost,abortConnect=False");
            Environment.SetEnvironmentVariable("ENABLE_CACHING", "true");
            // Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            _webApplicationFactory = new WebApplicationFactory<Program>();
            _serviceScope = _webApplicationFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            _httpClient = _webApplicationFactory.CreateClient();
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            _serviceScope.Dispose();
            _webApplicationFactory.Dispose();
        }

        [Fact]
        [Trait("Category", nameof(Api))]
        public async Task GetSubreddits()
        {
            const string endpoint = "http://localhost:82/api/pushshift/subreddit-names";

            // TODO: anyway to get rid of this delay
            // PushshiftBackgroundService needs time to gather data
            await Task.Delay(TimeSpan.FromSeconds(30));

            var response = await _httpClient.GetFromJsonAsync<IReadOnlyList<string>>(endpoint)
                .ConfigureAwait(false);

            Assert.True(response.Count > 0);
        }

        [Fact]
        [Trait("Category", nameof(Api))]
        public async Task Get_All_Reddit_Comments()
        {
            const string path = "http://localhost:82/api/pushshift/get-all-reddit-comments";

            // TODO: anyway to get rid of this delay
            // PushshiftBackgroundService needs time to gather data
            await Task.Delay(TimeSpan.FromSeconds(30));

            var response = await _httpClient.GetFromJsonAsync<IReadOnlyList<RedditComment>>(path)
                .ConfigureAwait(false);

            Assert.True(response.Count > 0);
        }
    }
}