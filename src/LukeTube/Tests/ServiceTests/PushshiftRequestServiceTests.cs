using LukeTube.Data;
using LukeTube.Services;
using Moq;
using LukeTube.Models.Pushshift;
using LukeTube.PollyPolicies;
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
            services.AddHttpClient<IPushshiftRequestService, PushshiftRequestService>("PushshiftServiceClient", client =>
                {
                    client.BaseAddress = new Uri("https://api.pushshift.io/");
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(2))
                .AddPolicyHandler(PushshiftPolicies.GetWaitAndRetryPolicy())
                .AddPolicyHandler(PushshiftPolicies.GetRateLimitPolicy());

            var httpClientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

            _pushshiftRequestService = new PushshiftRequestService(
                _loggerMock.Object,
                _pushshiftRepositoryMock.Object,
                httpClientFactory);
        }

        [Fact]
        public void FindYoutubeIdTest_ShouldReturnCorrectSizeId()
        {
            const string youtubeLink = @"https://www.youtube.com/watch?v=fe4Yf-0Wm4U";
            const int expectedLength = 11;

            var result = _pushshiftRequestService.FindYoutubeId(youtubeLink);

            Assert.Equal(expectedLength, result.Length);
        }

        [Fact]
        public void FindYoutubeIdTest_EmptyString()
        {
            var result = _pushshiftRequestService.FindYoutubeId(string.Empty);
            Assert.Empty(result);
        }
        
        [Fact]
        public async Task GetMeta()
        {
            var meta = await _pushshiftRequestService.GetPushshiftQueryResults<MetaResponse>("meta");
            Assert.True(meta.ClientAcceptsJson);
        }

        [Fact]
        public async Task GetComments()
        {
            var rawComments = await _pushshiftRequestService.GetPushshiftQueryResults<CommentResponse>("comment", new SearchOptions
            {
                Subreddit = "aviation",
                Query = "www.youtube.com/watch", // TODO: separate out the query for the other link and score
                Before = "24h",
                After = "0h",
                Size = 10
            });
            
            Assert.NotNull(rawComments);
        }

        [Fact]
        public async Task GetSubmissions()
        {
            var rawComments = await _pushshiftRequestService.GetPushshiftQueryResults<SubmissionResponse>("submission", new SearchOptions
            {
                Subreddit = "aviation",
                Query = "www.youtube.com/watch", // TODO: separate out the query for the other link and score
                Before = "24h",
                After = "0h",
                Size = 10
            });
            
            Assert.NotNull(rawComments);
        }
    }
}