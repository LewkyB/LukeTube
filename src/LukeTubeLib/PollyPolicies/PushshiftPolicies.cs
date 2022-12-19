using LukeTubeLib.Diagnostics;
using Polly;
using Polly.Contrib.WaitAndRetry;
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
            var maxDelay = TimeSpan.FromSeconds(45);
            var delay = Backoff.DecorrelatedJitterBackoffV2(
                    medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 5, seed: 100)
                .Select(s => TimeSpan.FromTicks(Math.Min(s.Ticks, maxDelay.Ticks)));

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => (int)response.StatusCode == 429)
                .Or<RateLimitRejectedException>()
                .WaitAndRetryAsync(
                    delay,
                    onRetry: (response, delay, retryCount, context) =>
                    {
                        PushshiftRequestCounterSource.Log.AddRetryRequest(retryCount);
                    });
            // .WaitAndRetryAsync(
            // retryCount: 5,
            // sleepDurationProvider: attempt => TimeSpan.FromSeconds(0.25 * Math.Pow(2, attempt)),
            // onRetry: (response, delay, retryCount, context) =>
            // {
            //     PushshiftRequestCounterSource.Log.AddRetryRequest(retryCount);
            // });
        }
    }
}