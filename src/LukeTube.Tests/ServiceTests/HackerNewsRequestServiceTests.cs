using LukeTubeLib.Models.HackerNews;
using Moq;
using LukeTubeLib.Repositories;
using LukeTubeLib.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace LukeTube.Tests.ServiceTests
{
    public class HackerNewsRequestServiceTests
    {
        private readonly Mock<ILogger<HackerNewsRequestService>> _loggerMock = new();
        private readonly Mock<IHackerNewsRepository> _HackerNewsRepositoryMock = new();
        private readonly HackerNewsRequestService _HackerNewsRequestService;

        public HackerNewsRequestServiceTests()
        {
            const string hackerNewsBaseAddress = "https://hn.algolia.com/api/v1/";
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(hackerNewsBaseAddress);

            _HackerNewsRequestService = new HackerNewsRequestService(_loggerMock.Object, httpClient);
        }

        [Fact]
        public async Task GetComments()
        {
            // var hackerNewsQueryResults = await _HackerNewsRequestService.GetHackerNewsQueryResults<HackerNewsResponse>(
            var hackerNewsQueryResults = await _HackerNewsRequestService.GetHackerNewsQueryResults(
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
        //                 var result =  await videoClient.GetAsync(redditComment.YoutubeId);
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