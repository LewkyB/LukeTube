using luke_site_mvc.Models.PsawSearchOptions;
using luke_site_mvc.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using luke_site_mvc.Data.Entities.PsawEntries;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using luke_site_mvc.Services.PollyPolicies;
using System;

namespace luke_site_mvc.Tests.ServiceTests
{
    public class PsawServiceTests
    {
        // TODO: what does this do?
        private readonly ITestOutputHelper _output;

        private readonly Mock<ILogger<PsawService>> _loggerMock;
        private readonly IPsawService _psawService;

        public PsawServiceTests(ITestOutputHelper output)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddHttpClient("PushshiftClient")
                .SetHandlerLifetime(TimeSpan.FromMinutes(2))
                .AddPolicyHandler(PushshiftPolicies.GetWaitAndRetryPolicy())
                .AddPolicyHandler(PushshiftPolicies.GetRateLimitPolicy());
            _output = output;

            _loggerMock = new Mock<ILogger<PsawService>>();
            IHttpClientFactory _httpClientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

            _psawService = new PsawService(_loggerMock.Object, _httpClientFactory);
        }

        [Fact]
        public async Task GetMeta()
        {
            var meta = await _psawService.GetMeta();
            Assert.True(meta.ClientAcceptsJson);
        }

        [Fact]
        public async Task GetSubmission()
        {
            var submmissions = await _psawService.Search<SubmissionEntry>(new SearchOptions
            {
                Subreddit = "game",
                Size = 1
            });

            Assert.Single(submmissions);
        }

        [Fact]
        public async Task GetSubmissions()
        {
            var submmissions = await _psawService.Search<SubmissionEntry>(new SearchOptions
            {
                Subreddit = "game",
                Size = 100
            });

            Assert.Equal(100, submmissions.Length);
            Assert.DoesNotContain(submmissions.GroupBy(s => s.Id), g => g.Count() > 1);
        }

        [Fact]
        public async Task GetSubmissionCommentIds()
        {
            string[] ids = await _psawService.GetSubmissionCommentIds("a2df38");

            Assert.True(ids.Length > 2000);
        }

        [Fact]
        public async Task GetComments()
        {
            var comments = await _psawService.Search<CommentEntry>(new SearchOptions
            {
                Subreddit = "game",
                Size = 100
            });

            Assert.Equal(100, comments.Length);
            Assert.True(comments.All(c => c.Subreddit == "game"));
        }

        // TODO: flakey test, at last run was only fetching 488 comments
        //[Fact]
        //public async Task GetSubmissionComments()
        //{
        //    const string submissionId = "a2df38";

        //    var commentIds = (await _psawService.GetSubmissionCommentIds(submissionId)).Take(500).ToArray();

        //    // Only taking 500 because more would result in a [Request Line is too large (8039 > 4094)] error
        //    var comments = await _psawService.Search<CommentEntry>(new SearchOptions
        //    {
        //        Ids = commentIds
        //    });

        //    Assert.Equal(500, comments.Length);
        //}

        // TODO: fix this test, it runs way too long, make it run for shorter period
        //[Fact]
        //public async Task RateLimit()
        //{
        //    var client = new PsawClient();
        //    var options = new SearchOptions
        //    {
        //        Subreddit = "game",
        //        Size = 1
        //    };

        //    var sw = Stopwatch.StartNew();

        //    for (int i = 0; i < 180; i++)
        //    {
        //        await client.Search<SubmissionEntry>(options);
        //        _output.WriteLine(i + " done in " + sw.Elapsed.TotalMilliseconds);
        //    }

        //    // 180 requests should end in more than 2 minutes
        //    Assert.True(sw.Elapsed.TotalSeconds > 120);
        //}

        [Fact]
        public void SearchOptionsToArgs1()
        {
            var options = new SearchOptions
            {
                Query = "games",
                Size = 2500
            };

            var args = options.ToArgs();
            Assert.Equal(4, args.Count);
            Assert.Equal("size=1000", args[1]);
        }

        [Fact]
        public void SearchOptionsToArgs2()
        {
            var options = new SubmissionSearchOptions
            {
                Query = "game",
                QueryNot = "video",
                Stickied = true,
                NumComments = ">100",
                Fields = new[] { "author", "title", "permalink" },
                Size = 2500
            };

            var args = options.ToArgs();
            Assert.Equal(8, args.Count);
            Assert.Equal("size=1000", args[1]);
            Assert.Equal($"fields={string.Join(",", options.Fields)}", args[2]);
        }
    }
}
