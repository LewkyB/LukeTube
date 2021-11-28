using luke_site_mvc.Data;
using luke_site_mvc.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
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
        private readonly HttpClient _httpClient;
        private readonly IPsawService _psawService;
        private readonly Mock<ILogger<PsawService>> _loggerPsawServiceMock;

        // PushshiftService
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<ILogger<PushshiftService>> _loggerMock;
        public readonly PushshiftService _pushshiftService;

        public PushshiftServiceTests()
        {
            // PsawService
            _httpClient = new HttpClient();
            _loggerPsawServiceMock = new Mock<ILogger<PsawService>>();
            _psawService = new PsawService(_httpClient, _loggerPsawServiceMock.Object);

            // TODO: mock SubredditRepository so that this becomes more of a unit test
            // SubredditRepository
            _configMock = new Mock<IConfiguration>();
            _repoLoggerMock = new Mock<ILogger<SubredditRepository>>();
            _chatroomContext = new SubredditContext(new DbContextOptions<SubredditContext>());
            _subredditRepository = new SubredditRepository(_configMock.Object, _repoLoggerMock.Object, _chatroomContext);

            // PushshiftService
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

        [Fact]
        public async Task GetLinksFromCommentsAsync_NotNull()
        {
            string subreddit = "videos";
            var result = await _pushshiftService.GetUniqueRedditComments(subreddit, daysToGet: 5, numEntriesPerDay: 10);

            Assert.NotNull(result[0]);
        }
    }
}
