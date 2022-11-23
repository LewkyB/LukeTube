using Polly;
using Polly.Extensions.Http;
using Polly.RateLimit;

namespace LukeTubeLib.PollyPolicies
{
    public static class PushshiftPolicies
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRateLimitPolicy()
        {
            return Policy
                .RateLimitAsync<HttpResponseMessage>(numberOfExecutions: 120, TimeSpan.FromSeconds(60));
        }
        public static IAsyncPolicy<HttpResponseMessage> GetWaitAndRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => (int)response.StatusCode == 429)
                .Or<RateLimitRejectedException>()
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(0.25 * Math.Pow(2, attempt)));
        }
    }
}
