using luke_site_mvc.Data;
using luke_site_mvc.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.Http;
using Xunit;

namespace luke_site_mvc.Tests.ServiceTests
{
    public class PushshiftServiceTests
    {
        // SubredditRepository
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<ILogger<SubredditRepository>> _repoLoggerMock;
        private readonly SubredditContext _chatroomContext;
        private readonly ISubredditRepository _subredditRepository;

        // PsawService
        private readonly IPsawService _psawService;
        private readonly Mock<ILogger<PsawService>> _loggerPsawServiceMock;

        // PushshiftService
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<ILogger<PushshiftService>> _loggerMock;
        public readonly PushshiftService _pushshiftService;

        private readonly HttpClient _httpClient;

        public PushshiftServiceTests()
        {
            _httpClient = new HttpClient();

            _loggerPsawServiceMock = new Mock<ILogger<PsawService>>();
            _psawService = new PsawService(_httpClient, _loggerPsawServiceMock.Object);

            // TODO: do I need to mock this instead of use the real thing?
            _configMock = new Mock<IConfiguration>();
            _repoLoggerMock = new Mock<ILogger<SubredditRepository>>();
            _chatroomContext = new SubredditContext(new DbContextOptions<SubredditContext>());
            _subredditRepository = new SubredditRepository(_configMock.Object, _repoLoggerMock.Object, _chatroomContext);

            _cacheMock = new Mock<IDistributedCache>();
            _loggerMock = new Mock<ILogger<PushshiftService>>();

            _pushshiftService = new PushshiftService(
                _loggerMock.Object,
                _cacheMock.Object,
                _psawService,
                _subredditRepository);
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

        // TODO: fix broken tests
        //[Fact]
        //public async Task GetLinksFromCommentsAsync_NotNull()
        //{
        //    string subreddit = "videos";
        //    var result = await _pushshiftService.GetLinksFromCommentsAsync(subreddit);

        //    Assert.NotNull(result);
        //}

        // checks to make sure comments links are ordered by highest score
        // sometimes all the comments will have a score of 1, hence the >=
        //[Fact]
        //public async Task GetLinksFromCommentsAsync_EnsureOrderedByScoreDesc()
        //{
        //    string subreddit = "videos";
        //    var result = await _pushshiftService.GetLinksFromCommentsAsync(subreddit);

        //    // TODO: should check more values?
        //    if (result.Count > 1)
        //    {
        //        Assert.True(result[0].Score >= result[1].Score);
        //    }
        //}
    }
}
