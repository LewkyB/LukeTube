using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using PsawSharp.Entries;
using PsawSharp.Requests;
using PsawSharp.Requests.Options;
using Xunit;
using Xunit.Abstractions;

namespace PsawSharp.Tests
{
    public class PsawServiceTests
    {
        private readonly ITestOutputHelper _output;
        private readonly IPsawService _psawService;
        private readonly Mock<ILogger<PsawService>> _loggerMock;

        public PsawServiceTests(ITestOutputHelper output)
        {
            _output = output;

            _loggerMock = new Mock<ILogger<PsawService>>();
            _psawService = new PsawService(new HttpClient(), _loggerMock.Object);
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

        [Fact]
        public async Task GetSubmissionComments()
        {
            const string submissionId = "a2df38";

            var commentIds = (await _psawService.GetSubmissionCommentIds(submissionId)).Take(500).ToArray();

            // Only taking 500 because more would result in a [Request Line is too large (8039 > 4094)] error
            var comments = await _psawService.Search<CommentEntry>(new SearchOptions
            {
                Ids = commentIds
            });

            Assert.Equal(500, comments.Length);
        }

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

        //[Fact]
        //public async Task ProxyUsage()
        //{
        //    var client = new PsawClient(new RequestsManagerOptions
        //    {
        //        ProxyAddress = "178.217.194.175:49850"
        //    });

        //    var meta = await client.GetMeta();
        //    Assert.Equal("178.217.194.175", meta.SourceIp);
        //    Assert.Equal("PL", meta.ClientRequestHeaders.CfIpCountry);
        //}

    }
}
