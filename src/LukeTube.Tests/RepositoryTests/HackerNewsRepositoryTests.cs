using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using JetBrains.Annotations;
using LukeTubeLib.Repositories;
using LukeTubeLib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace LukeTube.Tests.RepositoryTests;

[UsedImplicitly]
public sealed class HackerNewsRepositoryTests : IAsyncLifetime
{
    private readonly IDockerContainer _postgreSqlContainer;


    public HackerNewsRepositoryTests()
    {
        var postgreSqlConfiguration = new PostgreSqlTestcontainerConfiguration();
        postgreSqlConfiguration.Username = "postgres";
        postgreSqlConfiguration.Password = "postgres";
        postgreSqlConfiguration.Database = "HackerNewsDb";

        _postgreSqlContainer = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(postgreSqlConfiguration)
            .WithPortBinding(5432, 5432)
            // .WithVolumeMount(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath + "postgres-data", "/var/lib/postgresql/data")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync()
            .ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync()
            .ConfigureAwait(false);
    }

    public sealed class RepoTests : IClassFixture<PushshiftRepositoryTests>
    {
        private readonly Mock<ILogger<IHackerNewsRepository>> _loggerMock = new Mock<ILogger<IHackerNewsRepository>>();

        private readonly HackerNewsContext _hackerNewsContext;
        private readonly IHackerNewsRepository _hackerNewsRepository;
        private readonly IHackerNewsRequestService _hackerNewsRequestService;

        private const string _postgreSqlConnectionString = "host=localhost;database=HackerNewsDb;username=postgres;password=postgres;";
        private const string hackerNewsBaseAddress = "https://hn.algolia.com/api/v1/";

        private readonly DbContextOptionsBuilder<HackerNewsContext> _dbOptionsBuilder = new DbContextOptionsBuilder<HackerNewsContext>()
            .UseNpgsql(_postgreSqlConnectionString)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging();

        public RepoTests()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(hackerNewsBaseAddress);
            _hackerNewsContext = new HackerNewsContext(_dbOptionsBuilder.Options);
            _hackerNewsRepository = new HackerNewsRepository(_hackerNewsContext, NullLogger<HackerNewsRepository>.Instance);
            _hackerNewsRequestService = new HackerNewsRequestService(NullLogger<HackerNewsRequestService>.Instance, httpClient);
        }
    }
}