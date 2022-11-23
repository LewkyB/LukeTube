using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using JetBrains.Annotations;
using LukeTubeLib.Models.Pushshift;
using LukeTubeLib.PollyPolicies;
using LukeTubeLib.Repositories;
using LukeTubeLib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NLog;
using Xunit;
using YoutubeExplode.Videos;

namespace LukeTube.Tests.RepositoryTests;

[UsedImplicitly]
public sealed class PushshiftRepositoryTests : IAsyncLifetime
{
    private readonly IDockerContainer _postgreSqlContainer;
    private static string _postgreSqlConnectionString;

    public PushshiftRepositoryTests()
    {
        var postgreSqlConfiguration = new PostgreSqlTestcontainerConfiguration();
        postgreSqlConfiguration.Username = "postgres";
        postgreSqlConfiguration.Password = "postgres";
        postgreSqlConfiguration.Database = "SubredditDb";
        _postgreSqlConnectionString =
            $"host=localhost;database={postgreSqlConfiguration.Database};username={postgreSqlConfiguration.Username};password={postgreSqlConfiguration.Password}";

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
        private readonly IPushshiftRepository _pushshiftRepository;

        private DbContextOptionsBuilder<PushshiftContext> dbOption = new DbContextOptionsBuilder<PushshiftContext>()
            .UseNpgsql(_postgreSqlConnectionString)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging();

        // options.EnableDetailedErrors();
        // options.EnableSensitiveDataLogging();
        // options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRINGS__POSTGRESQL"));
        private readonly Mock<ILogger<PushshiftRepository>> _loggerMock = new Mock<ILogger<PushshiftRepository>>();

        public RepoTests()
        {
            _pushshiftRepository = new PushshiftRepository(new PushshiftContext(dbOption.Options), _loggerMock.Object);
        }

        [Fact]
        public async Task Test()
        {
            var redditComments = new List<RedditComment>
            {
                new()
                {
                    YoutubeLinkId = "fdssdferrtf",
                    Subreddit = "movies",
                    Score = 5,
                    Permalink = "",
                    CreatedUTC = DateTime.Now.ToUniversalTime(),
                    RetrievedUTC = DateTime.Now.ToUniversalTime(),
                },
                new()
                {
                    YoutubeLinkId = "fdssdferrtf",
                    Subreddit = "movies",
                    Score = 5,
                    Permalink = "",
                    CreatedUTC = DateTime.Now.ToUniversalTime(),
                    RetrievedUTC = DateTime.Now.ToUniversalTime(),
                }
            };

            await _pushshiftRepository.SaveRedditComments(redditComments);
            await _pushshiftRepository.SaveRedditComments(redditComments);

            var allComments = await _pushshiftRepository.GetAllRedditComments();

            Assert.NotEmpty(allComments);
        }

        [Fact]
        public async Task NewVideoTable()
        {
            var loggerMock = new Mock<ILogger<PushshiftRequestService>>();
            IServiceCollection services = new ServiceCollection();
            services.AddHttpClient<IPushshiftRequestService, PushshiftRequestService>("PushshiftRequestServiceClient",
                    client => { client.BaseAddress = new Uri("https://api.pushshift.io/"); })
                .SetHandlerLifetime(TimeSpan.FromMinutes(2))
                .AddPolicyHandler(PushshiftPolicies.GetWaitAndRetryPolicy())
                .AddPolicyHandler(PushshiftPolicies.GetRateLimitPolicy());

            var httpClientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
            var _pushshiftRequestService = new PushshiftRequestService(loggerMock.Object, _pushshiftRepository, httpClientFactory);

            var videoHttpClient = new HttpClient();
            var videoClient = new VideoClient(videoHttpClient);

            var startDate = 0;
            var searchOptionsNoDates = _pushshiftRequestService.BuildSearchOptionsNoDates("www.youtube.com", 500);
            var filledOutOptions = _pushshiftRequestService
                .AddBeforeAndAfter(searchOptionsNoDates, startDate)
                .Take(3);

            foreach (var searchOption in filledOutOptions)
            {
                // var redditComments = await _pushshiftRequestService.GetUniqueRedditComments(searchOption, client);
                var redditComments = (await _pushshiftRequestService.GetUniqueRedditComments(searchOption, null)).ToList();

                // var newlySavedComments = await _pushshiftRepository.SaveRedditComments(redditComments);

                // var videos = new List<Video>();
                // foreach (var redditComment in newlySavedComments)
                foreach (var redditComment in redditComments)
                {
                    try
                    {
                        var result =  await videoClient.GetAsync(redditComment.YoutubeLinkId);
                        if (result is not null)
                        {
                            redditComment.VideoModel = VideoModelHelper.MapVideoEntity(result);
                        }
                        // videos.Add(result);
                    }
                    catch(Exception ex)
                    {}
                }

                var newlySavedComments = await _pushshiftRepository.SaveRedditComments(redditComments);
                // await _pushshiftRepository.SaveVideos();
            }
            var pushshiftContext = new PushshiftContext(dbOption.Options);
            var comments = await pushshiftContext.RedditComments.ToListAsync();
            var videos = await pushshiftContext.Videos.ToListAsync();
            var combo = await pushshiftContext.RedditComments
                .Include(x => x.VideoModel)
                .Include(x => x.VideoModel.Author)
                .Include(x => x.VideoModel.EngagementModel)
                .Include(x => x.VideoModel.Thumbnails)
                .ToListAsync();

            var records = await _pushshiftRepository.GetAllRedditComments();
            Assert.True(records.Count > 0);
        }
    }
}