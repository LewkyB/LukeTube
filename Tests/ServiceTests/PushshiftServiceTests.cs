using System.Net.Http;
using System.Threading.Tasks;
using luke_site_mvc.Data;
using luke_site_mvc.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using PsawSharp.Requests;
using Xunit;

namespace luke_site_mvc.Tests
{
    public class PushshiftServiceTests
    {
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<ILogger<PushshiftService>> _loggerMock;
        private readonly SubredditContext _chatroomContext;

        private readonly IPsawService _psawService;
        private readonly Mock<ILogger<PsawService>> _loggerPsawServiceMock;

        public readonly PushshiftService _pushshiftService;

        public PushshiftServiceTests()
        {
            _cacheMock = new Mock<IDistributedCache>();
            _loggerMock = new Mock<ILogger<PushshiftService>>();

            _loggerPsawServiceMock = new Mock<ILogger<PsawService>>();

            // TODO: should i be mocking this instead?
            _psawService = new PsawService(new HttpClient(), _loggerPsawServiceMock.Object);

            // TODO: i need to mock this instead of using the real thing
            _chatroomContext = new SubredditContext(new DbContextOptions<SubredditContext>());

            _pushshiftService = new PushshiftService(
                _loggerMock.Object,
                _cacheMock.Object,
                _chatroomContext,
                _psawService);
        }

        [Fact]
        public void FindYoutubeIdTest_ShouldReturnCorrectSizeId()
        {
            string youtube_link = @"https://www.youtube.com/watch?v=fe4Yf-0Wm4U";
            int expectedLength = 11;

            var result = _pushshiftService.FindYoutubeId(youtube_link);

            Assert.Equal(result.Length, expectedLength);
        }

        [Fact]
        public void FindYoutubeIdTest_EmptyString()
        {
            string youtube_link = "";

            var result = _pushshiftService.FindYoutubeId(youtube_link);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetLinksFromCommentsAsync_NotNull()
        {
            string subreddit = "videos";
            var result = await _pushshiftService.GetLinksFromCommentsAsync(subreddit);

            Assert.NotNull(result);
        }

        [Fact]
        // checks to make sure comments links are ordered by highest score
        // sometimes all the comments will have a score of 1, hence the >=
        public async Task GetLinksFromCommentsAsync_EnsureOrderedByScoreDesc()
        {
            string subreddit = "videos";
            var result = await _pushshiftService.GetLinksFromCommentsAsync(subreddit);

            // TODO: should check more values?
            if (result.Count > 1)
            {
                Assert.True(result[0].Score >= result[1].Score);
            }
        }

    }
}
