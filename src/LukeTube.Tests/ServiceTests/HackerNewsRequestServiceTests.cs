using System.Threading.RateLimiting;
using LukeTube.Services;
using LukeTubeLib;
using LukeTubeLib.Models.HackerNews;
using Moq;
using LukeTubeLib.Models.Pushshift;
using LukeTubeLib.PollyPolicies;
using LukeTubeLib.Repositories;
using LukeTubeLib.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using YoutubeExplode.Videos;

namespace LukeTube.Tests.ServiceTests
{
    public class HackerNewsRequestServiceTests
    {
        private readonly Mock<ILogger<HackerNewsRequestService>> _loggerMock = new();
        private readonly Mock<IHackerNewsRepository> _HackerNewsRepositoryMock = new();
        private readonly HackerNewsRequestService _HackerNewsRequestService;

        public HackerNewsRequestServiceTests()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddHttpClient<IHackerNewsRequestService, HackerNewsRequestService>("HackerNewsRequestServiceClient",
                    client => { client.BaseAddress = new Uri("https://api.pushshift.io/"); })
                .SetHandlerLifetime(TimeSpan.FromMinutes(2))
                .AddPolicyHandler(PushshiftPolicies.GetWaitAndRetryPolicy())
                .AddPolicyHandler(PushshiftPolicies.GetRateLimitPolicy());

            var httpClientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

            _HackerNewsRequestService = new HackerNewsRequestService(httpClientFactory, _loggerMock.Object);
        }

        [Theory]
        [InlineData(@"https://www.youtube.com/watch?v=fe4Yf-0Wm4U", 11)]
        public void FindYoutubeIdTest_ShouldReturnCorrectSizeId(string youtubeLink, int expectedLength)
        {
            var result = YoutubeUtilities.FindYoutubeId(youtubeLink);
            Assert.True(result.Count is 1);
            Assert.Equal(expectedLength, result[0].Length);
        }

        [Theory]
        [InlineData(@"https://www.youtube.com/watch?v=fe4Yf-0Wm4U https://www.youtube.com/watch?v=fg6pf-0Wm4U", 2, 11)]
        public void FindYoutubeIdTest_ShouldReturnMultipleIds(string body, int expectedCount, int expectedLength)
        {
            var result = YoutubeUtilities.FindYoutubeId(body);
            Assert.Equal(expectedCount, result.Count);

            foreach (var res in result)
            {
                Assert.Equal(expectedLength, res.Length);
            }
        }

        [Fact]
        public void FindYoutubeIdTest_EmptyString()
        {
            var result = YoutubeUtilities.FindYoutubeId(string.Empty);
            Assert.Empty(result);
        }

        [Theory]
        [InlineData("youtube.com/watch?v=yIVRs6YSbOM", "yIVRs6YSbOM")]
        [InlineData("youtu.be/yIVRs6YSbOM", "yIVRs6YSbOM")]
        [InlineData("youtube.com/embed/yIVRs6YSbOM", "yIVRs6YSbOM")]
        [InlineData("youtube.com/shorts/sKL1vjP0tIo", "sKL1vjP0tIo")]
        public void Video_ID_can_be_parsed_from_a_URL_string(string videoUrl, string expectedVideoId)
        {
            var result = YoutubeUtilities.FindYoutubeId(videoUrl);
            Assert.True(result.Count is 1);
            Assert.Equal(expectedVideoId, result[0]);
        }

        [Theory]
        [InlineData("9bZkp7q19f0")]
        [InlineData("_kmeFXjjGfk")]
        [InlineData("AI7ULzgf8RU")]
        public void Video_ID_can_be_parsed_from_an_ID_string(string videoId)
        {
            var result = YoutubeUtilities.FindYoutubeId(videoId);
            Assert.True(result.Count is 1);
            Assert.Equal(videoId, result[0]);
        }

        [Theory]
        [InlineData("")]
        [InlineData("pI2I2zqzeK")]
        [InlineData("pI2I2z zeKg")]
        [InlineData("youtube.com/xxx?v=pI2I2zqzeKg")]
        [InlineData("youtu.be/watch?v=xxx")]
        [InlineData("youtube.com/embed/")]
        public void Video_ID_cannot_be_parsed_from_an_invalid_string(string videoId)
        {
            var result = YoutubeUtilities.FindYoutubeId(videoId);

            Assert.True(result.Count is 0);
        }

        [Fact]
        public async Task GetComments()
        {
            var hackerNewsQueryResults = await _HackerNewsRequestService.GetHackerNewsQueryResults<HackerNewsResponse>(
                "search",
                new HackerNewsSearchOptions
                {
                    Query = "www.youtube.com/watch", // TODO: separate out the query for the other link and score
                    After = "0",
                    Before = "100000000",
                    HitsPerPage = 1000,
                },
                CancellationToken.None);

            Assert.True(hackerNewsQueryResults.Hits.Count > 0);
        }

        // [Theory]
        // [InlineData("youtube.com", 100, "24h", "0h")]
        // public void BuildSearchOptions(string query, int numEntriesPerDay, string before, string after)
        // {
        //     var results = _HackerNewsRequestService.BuildSearchOptions(query, numEntriesPerDay, before, after);
        //     Assert.True(results.Count > 0);
        // }
        //
        // [Theory]
        // [InlineData("youtube.com", 365, 100)]
        // public void GetSearchOptions(string query, int numOfDays, int maxNumComments)
        // {
        //     var results = _HackerNewsRequestService.GetSearchOptions(query, numOfDays, maxNumComments);
        //     Assert.True(results.Count > 0);
        // }
        //
        // [Fact]
        // // [InlineData("youtube.com", 365, 100)]
        // // public void GetSearchOptionsNoDates(string query, int numOfDays, int maxNumComments)
        // public void GetSearchOptionsNoDates()
        // {
        //     var options = _HackerNewsRequestService.BuildSearchOptionsNoDates("wwww.youtube.com", 500);
        //     var results = _HackerNewsRequestService.AddBeforeAndAfter(options, 0);
        //     Assert.True(results.Count > 0);
        // }

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
        //     // var searchOptions = _HackerNewsRequestService.GetSearchOptions("www.youtube.com/watch", 365, 100);
        //     var searchOptionsNoDates = _HackerNewsRequestService.BuildSearchOptionsNoDates("www.youtube.com", 500);
        //
        //     // using HttpClient client = new HttpClient(new ClientSideRateLimitedHandler(rateLimitOptions));
        //     using HttpClient client = new HttpClient(new ClientSideRateLimitedHandler(new TokenBucketRateLimiter(rateLimitOptions)));
        //     client.BaseAddress = new Uri("https://api.pushshift.io/");
        //
        //     int startDate = 0; // 0 is today, 1 is tomorrow
        //     while (true)
        //     {
        //         var filledOutOptions = _HackerNewsRequestService.AddBeforeAndAfter(searchOptionsNoDates, startDate);
        //         var videoHttpClient = new HttpClient();
        //         var videoClient = new VideoClient(videoHttpClient);
        //         foreach (var searchOption in filledOutOptions)
        //         {
        //             var redditComments =
        //                 await _HackerNewsRequestService.GetUniqueRedditComments(searchOption, client, CancellationToken.None);
        //
        //             foreach (var redditComment in redditComments)
        //             {
        //                 var result =  await videoClient.GetAsync(redditComment.YoutubeLinkId);
        //             }
        //             // if (redditComments.Count > 0) await _HackerNewsRepository.SaveRedditComments(redditComments);
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
        //         //             tasks.Add(_HackerNewsRequestService.GetUniqueRedditComments(searchOption, client));
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