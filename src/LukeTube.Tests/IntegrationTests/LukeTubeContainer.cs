using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Xunit;

namespace LukeTube.Tests.IntegrationTests;

[UsedImplicitly]
public sealed class LukeTubeContainer : HttpClient, IAsyncLifetime
{
    // private static readonly X509Certificate Certificate = new X509Certificate2(LukeTubeImage.CertificateFilePath, LukeTubeImage.CertificatePassword);

    private static readonly LukeTubeBackendImage LukeTubeBackendImage = new();
    private static readonly LukeTubeFrontendImage LukeTubeFrontendImage = new();
    private static readonly LukeTubeWorkerImage LukeTubeWorkerImage = new();

    private readonly IDockerNetwork _lukeTubeNetwork;

    private readonly IDockerContainer _postgreSqlContainer;
    private readonly IDockerContainer _redisContainer;

    private readonly IDockerContainer _lukeTubeBackendContainer;
    private readonly IDockerContainer _lukeTubeFrontendContainer;
    private readonly IDockerContainer _lukeTubeWorkerContainer;

    private readonly bool _frontendEnabled = false;

    public LukeTubeContainer()
    {
        // TestcontainersSettings.Logger = new MyLogger();

        const string lukeTubeDb = "lukeTubeDb";
        var postgreSqlConfiguration = new PostgreSqlTestcontainerConfiguration();
        postgreSqlConfiguration.Username = "postgres";
        postgreSqlConfiguration.Password = "postgres";
        postgreSqlConfiguration.Database = "SubredditDb";
        var postgreSqlConnectionString =
            $"host={lukeTubeDb};database={postgreSqlConfiguration.Database};username={postgreSqlConfiguration.Username};password={postgreSqlConfiguration.Password}";

        // redis container setup
        const string lukeTubeCache = "lukeTubeCache";
        var redisConfiguration = new RedisTestcontainerConfiguration();
        var redisConnectionString = $"{lukeTubeCache},abortConnect=False";

        _lukeTubeNetwork = new TestcontainersNetworkBuilder()
            .WithName(Guid.NewGuid().ToString("D"))
            .Build();

        _postgreSqlContainer = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(postgreSqlConfiguration)
            .WithNetwork(_lukeTubeNetwork)
            .WithNetworkAliases(lukeTubeDb)
            .Build();

        _redisContainer = new TestcontainersBuilder<RedisTestcontainer>()
            .WithDatabase(redisConfiguration)
            .WithNetwork(_lukeTubeNetwork)
            .WithNetworkAliases(lukeTubeCache)
            .Build();

        _lukeTubeBackendContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage(LukeTubeBackendImage)
            .WithNetwork(_lukeTubeNetwork)
            .WithPortBinding(82, 82)
            .WithExposedPort(82)
            .WithEnvironment("ASPNETCORE_URLS", "http://+:82")
            .WithEnvironment("CONNECTION_STRINGS__POSTGRESQL", postgreSqlConnectionString)
            .WithEnvironment("CONNECTION_STRINGS__REDIS", redisConnectionString)
            // .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(LukeTubeBackendImage.HttpPort))
            .Build();

        const string lukeTubeWorker = "lukeTubeWorker";
        _lukeTubeWorkerContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage(LukeTubeWorkerImage)
            .WithNetwork(_lukeTubeNetwork)
            .WithNetworkAliases(lukeTubeWorker)
            .WithEnvironment("CONNECTION_STRINGS__POSTGRESQL", postgreSqlConnectionString)
            .Build();

        if (_frontendEnabled)
        {
            _lukeTubeFrontendContainer = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage(LukeTubeFrontendImage)
                .WithNetwork(_lukeTubeNetwork)
                .WithPortBinding(81, 80)
                .WithExposedPort(81)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(LukeTubeFrontendImage.HttpPort))
                .Build();
        }
    }
    // static LukeTubeBackendImage()
    // {
    //     TestcontainersSettings.Logger = new MyLogger();
    // }
    //
    public sealed class MyLogger : ILogger, IDisposable
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            File.AppendAllText("diagnosticContainer.log", formatter.Invoke(state, exception));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return this;
        }

        public void Dispose()
        {
        }
    }

    public async Task InitializeAsync()
    {
        await _lukeTubeNetwork.CreateAsync()
            .ConfigureAwait(false);

        // await Task.WhenAll(LukeTubeBackendImage.InitializeAsync(), LukeTubeWorkerImage.InitializeAsync()).ConfigureAwait(false);

        await LukeTubeBackendImage.InitializeAsync()
            .ConfigureAwait(false);

        await LukeTubeWorkerImage.InitializeAsync()
            .ConfigureAwait(false);

        // TODO: parallelize containers starting: when I use this, why does only the worker service start, then turn off
        // await Task.WhenAll(
        //     _lukeTubeBackendContainer.StartAsync(),
        //     _postgreSqlContainer.StartAsync(),
        //     _redisContainer.StartAsync(),
        //     _lukeTubeWorkerContainer.StartAsync()).ConfigureAwait(false);
        await _lukeTubeBackendContainer.StartAsync()
            .ConfigureAwait(false);

        await _postgreSqlContainer.StartAsync()
            .ConfigureAwait(false);

        await _redisContainer.StartAsync()
            .ConfigureAwait(false);

        await _lukeTubeWorkerContainer.StartAsync()
            .ConfigureAwait(false);

        if (_frontendEnabled)
        {
            await LukeTubeFrontendImage.InitializeAsync()
                .ConfigureAwait(false);

            await _lukeTubeFrontendContainer.StartAsync()
                .ConfigureAwait(false);
        }
    }

    public async Task DisposeAsync()
    {
        await LukeTubeBackendImage.DisposeAsync()
            .ConfigureAwait(false);

        await _lukeTubeBackendContainer.DisposeAsync()
            .ConfigureAwait(false);

        await LukeTubeWorkerImage.DisposeAsync()
            .ConfigureAwait(false);

        await _lukeTubeWorkerContainer.DisposeAsync()
            .ConfigureAwait(false);

        if (_frontendEnabled)
        {
            await LukeTubeFrontendImage.DisposeAsync()
                .ConfigureAwait(false);

            await _lukeTubeFrontendContainer.DisposeAsync()
                .ConfigureAwait(false);
        }

        await _postgreSqlContainer.DisposeAsync()
            .ConfigureAwait(false);

        await _redisContainer.DisposeAsync()
            .ConfigureAwait(false);

        await _lukeTubeNetwork.DeleteAsync()
            .ConfigureAwait(false);
    }

    public void SetBackendBaseAddress()
    {
        try
        {
            var uriBuilder = new UriBuilder("http", _lukeTubeBackendContainer.Hostname, _lukeTubeBackendContainer.GetMappedPublicPort(LukeTubeBackendImage.HttpPort));
            BaseAddress = uriBuilder.Uri;
        }
        catch
        {
            // Set the base address only once.
        }
    }
}