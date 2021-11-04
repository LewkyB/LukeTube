using System.Threading.Tasks;
using luke_site_mvc.Data;
using luke_site_mvc.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace luke_site_mvc.Tests
{
    public class PushshiftServiceTests
    {
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<ILogger<PushshiftService>> _loggerMock;
        private readonly ChatroomContext _chatroomContext;

        public readonly PushshiftService _pushshiftService;

        public PushshiftServiceTests()
        {
            _cacheMock = new Mock<IDistributedCache>();
            _loggerMock = new Mock<ILogger<PushshiftService>>();

            // TODO: i need to mock this instead of using the real thing
            _chatroomContext = new ChatroomContext(new DbContextOptions<ChatroomContext>());

            _pushshiftService = new PushshiftService(
                _loggerMock.Object,
                _cacheMock.Object,
                _chatroomContext);
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
            // TODO: what if there isn't more than 0?
            Assert.True(result[0].Score >= result[1].Score);
        }

    }
}
