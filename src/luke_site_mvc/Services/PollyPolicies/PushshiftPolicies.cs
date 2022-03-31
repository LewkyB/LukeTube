using Polly;
using Polly.Extensions.Http;
using Polly.RateLimit;
using System;
using System.Net.Http;

namespace luke_site_mvc.Services.PollyPolicies
{
    public class PushshiftPolicies
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRateLimitPolicy()
        {
            // set rate limit policy
            return Policy
                .RateLimitAsync<HttpResponseMessage>(numberOfExecutions: 60, TimeSpan.FromSeconds(60));

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
