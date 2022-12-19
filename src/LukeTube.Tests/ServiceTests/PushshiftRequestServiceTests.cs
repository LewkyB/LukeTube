using Moq;
using LukeTubeLib.Models.Pushshift;
using LukeTubeLib.PollyPolicies;
using LukeTubeLib.Repositories;
using LukeTubeLib.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace LukeTube.Tests.ServiceTests
{
    public class PushshiftRequestServiceTests
    {
        private readonly Mock<ILogger<PushshiftRequestService>> _loggerMock = new();
        private readonly Mock<IPushshiftRepository> _pushshiftRepositoryMock = new();
        private readonly PushshiftRequestService _pushshiftRequestService;

        public PushshiftRequestServiceTests()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddHttpClient<IPushshiftRequestService, PushshiftRequestService>("PushshiftRequestServiceClient",
                    client => { client.BaseAddress = new Uri("https://api.pushshift.io/"); })
                .SetHandlerLifetime(TimeSpan.FromMinutes(2))
                .AddPolicyHandler(PushshiftPolicies.GetWaitAndRetryPolicy())
                .AddPolicyHandler(PushshiftPolicies.GetRateLimitPolicy());

            var httpClientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
            const string pushshiftBaseAddress = "https://api.pushshift.io/";
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(pushshiftBaseAddress);

            _pushshiftRequestService = new PushshiftRequestService(
                _loggerMock.Object,
                httpClient);
        }


        [Fact]
        public async Task GetMeta()
        {
            var meta = await _pushshiftRequestService.GetPushshiftQueryResults<MetaResponse>(
                "meta",
                null,
                CancellationToken.None);

            Assert.True(meta.ClientAcceptsJson);
        }

        [Theory]
        [InlineData("aviation")]
        [InlineData("programming")]
        public async Task GetComments(string subreddit)
        {
            var rawComments = await _pushshiftRequestService.GetPushshiftQueryResults<CommentResponse>(
                "comment",
                new PushshiftSearchOptions
                {
                    Subreddit = subreddit,
                    Query = "www.youtube.com/watch", // TODO: separate out the query for the other link and score
                    Before = "0h",
                    After = "10000h",
                    Size = 100
                },
                CancellationToken.None);

            Assert.True(rawComments.Data.Count > 0);
        }

        [Theory]
        [InlineData("aviation")]
        [InlineData("programming")]
        public async Task GetSubmissions(string subreddit)
        {
            var rawComments = await _pushshiftRequestService.GetPushshiftQueryResults<SubmissionResponse>(
                "submission",
                new PushshiftSearchOptions
                {
                    Subreddit = subreddit,
                    Query = "www.youtube.com/watch", // TODO: separate out the query for the other link and score
                    Before = "0h",
                    After = "10000h",
                    Size = 100
                },
                CancellationToken.None);

            Assert.True(rawComments.Data.Count > 0);
        }

        [Theory]
        [InlineData("youtube.com", 100, "24h", "0h")]
        public void BuildSearchOptions(string query, int numEntriesPerDay, string before, string after)
        {
            var results = _pushshiftRequestService.BuildSearchOptions(query, numEntriesPerDay, before, after);
            Assert.True(results.Count > 0);
        }

        [Fact]
        // [InlineData("youtube.com", 365, 100)]
        // public void GetSearchOptionsNoDates(string query, int numOfDays, int maxNumComments)
        public void GetSearchOptionsNoDates()
        {
            var options = _pushshiftRequestService.BuildSearchOptionsNoDates("wwww.youtube.com", 500);
            var results = _pushshiftRequestService.AddBeforeAndAfter(options, 0);
            Assert.True(results.Count > 0);
        }

        // [Fact]
        // public async Task Tester()
        // {
        //     TokenBucketRateLimiterOptions rateLimitOptions = new()
        //     {
        //         TokenLimit = 8,
        //         QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        //         QueueLimit = 3,
        //         ReplenishmentPeriod = TimeSpan.FromMilliseconds(1),
        //         TokensPerPeriod = 2,
        //         AutoReplenishment = true
        //     };
        //
        //     // var searchOptions = _pushshiftRequestService.GetSearchOptions("www.youtube.com/watch", 365, 100);
        //     var searchOptionsNoDates = _pushshiftRequestService.BuildSearchOptionsNoDates("www.youtube.com", 500);
        //
        //     // using HttpClient client = new HttpClient(new ClientSideRateLimitedHandler(rateLimitOptions));
        //     using HttpClient client = new HttpClient(new ClientSideRateLimitedHandler(new TokenBucketRateLimiter(rateLimitOptions)));
        //     client.BaseAddress = new Uri("https://api.pushshift.io/");
        //
        //     int startDate = 0; // 0 is today, 1 is tomorrow
        //     while (true)
        //     {
        //         var filledOutOptions = _pushshiftRequestService.AddBeforeAndAfter(searchOptionsNoDates, startDate);
        //         var videoHttpClient = new HttpClient();
        //         var videoClient = new VideoClient(videoHttpClient);
        //         foreach (var searchOption in filledOutOptions)
        //         {
        //             var redditComments =
        //                 await _pushshiftRequestService.GetUniqueRedditComments(searchOption, client, CancellationToken.None);
        //
        //             foreach (var redditComment in redditComments)
        //             {
        //                 var result =  await videoClient.GetAsync(redditComment.YoutubeId);
        //             }
        //             // if (redditComments.Count > 0) await _pushshiftRepository.SaveRedditComments(redditComments);
        //         }
        //
        //         startDate++;
        //
        //         // var chunked = filledOutOptions.Chunk(10);
        //         // foreach (var chunk in chunked)
        //         // {
        //         //     var tasks = new List<Task>();
        //         //     var semaphore = new SemaphoreSlim(10);
        //         //     foreach (var searchOption in chunk)
        //         //     {
        //         //         await semaphore.WaitAsync();
        //         //         try
        //         //         {
        //         //             // tasks.Add(GetPushshiftQueryResults<CommentResponse>("comment", searchOption));
        //         //             tasks.Add(_pushshiftRequestService.GetUniqueRedditComments(searchOption, client));
        //         //         }
        //         //         finally
        //         //         {
        //         //             semaphore.Release();
        //         //         }
        //         //
        //         //     }
        //         //     await Task.WhenAll(tasks);
        //         //     // tasks.FirstOrDefault().Result;
        //         //
        //         //     var redditComments = new List<RedditComment>();
        //         //     foreach (var task in tasks)
        //         //     {
        //         //         var results = ((Task<IReadOnlyList<RedditComment>>)task).Result;
        //         //         // redditComments.Add(task);
        //         //     }
        //         // }
        //         //
        //         // startDate++;
        //     }
        // }
    }
}