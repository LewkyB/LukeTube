using luke_site_mvc.Models.PsawSearchOptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Polly;
using PsawSharp.Entries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace luke_site_mvc.Services
{
    // TODO: get rid of this
    public static class RequestsConstants
    {
        public const string BaseAddress = "https://api.pushshift.io/";
        public const string SearchRoute = "reddit/{0}/search";
        public const string CommentIdsRoute = "reddit/submission/comment_ids/{0}";
        public const int MaxRequestsPerMinute = 60;
    }

    public interface IPsawService
    {
        Task<MetaEntry> GetMeta();
        Task<string[]> GetSubmissionCommentIds(string base36SubmissionId);
        Task<T[]> Search<T>(SearchOptions options = null) where T : IEntry;
    }

    public class PsawService : IPsawService
    {
        private readonly ILogger<PsawService> _logger;
        private readonly HttpClient _httpClient;

        public PsawService(HttpClient httpClient, ILogger<PsawService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<T[]> Search<T>(SearchOptions options = null) where T : IEntry
        {
            string type = typeof(T).Name.Replace("Entry", "").ToLower();
            string route = string.Format(RequestsConstants.BaseAddress + RequestsConstants.SearchRoute, type);
            var result = await ExecuteGet(route, options?.ToArgs());
            return result["data"].ToObject<T[]>();
        }

        public async Task<string[]> GetSubmissionCommentIds(string base36SubmissionId)
        {
            string route = string.Format(RequestsConstants.BaseAddress + RequestsConstants.CommentIdsRoute, base36SubmissionId);
            var result = await ExecuteGet(route);
            return result["data"].ToObject<string[]>();
        }

        public async Task<MetaEntry> GetMeta()
        {
            var result = await ExecuteGet(RequestsConstants.BaseAddress + "meta");
            return result.ToObject<MetaEntry>();
        }

        // TODO: clean up exceptions and logging in ExecuteGet
        private async Task<JToken> ExecuteGet(string route, List<string> args = null)
        {
            _logger.LogTrace($"Making HTTP request to URI: {ConstructUrl(route, args)}");

            // get rate limiting policy for HttpClient
            var resilencyStrategy = DefineAndRetrieveResiliencyStrategy();

            // apply rate limit policy to HttpClient
            var response = await resilencyStrategy.ExecuteAsync(() => _httpClient.GetAsync(ConstructUrl(route, args)));

            _logger.LogTrace($"HTTP response code: {nameof(response.StatusCode)}: {response.StatusCode}");

            // TODO: make this throw an exception on the asp.net page
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"HTTP request to {route} FAILED. Reponse StatusCode: {response.StatusCode}");
                throw new HttpRequestException($"request to {route} failed", null, response.StatusCode);
            }

            response.EnsureSuccessStatusCode();

            // Convert response to json
            string result = await response.Content.ReadAsStringAsync();
            return JToken.Parse(result);
        }

        private string ConstructUrl(string route, List<string> args) => args == null ? route : $"{route}?{ArgsToString(args)}";

        private static string ArgsToString(List<string> args) => args.Aggregate((x, y) => $"{x}&{y}");

        // TODO: unit test polly policy https://github.com/App-vNext/Polly/wiki/Unit-testing-with-Polly
        /// <summary>
        /// Creates a Polly-based resiliency strategy that helps deal with transient faults when communicating
        /// with the external (downstream) Computer Vision API service.
        /// source: http://www.thepollyproject.org/2018/03/06/policy-recommendations-for-azure-cognitive-services/
        /// </summary>
        /// <returns></returns>
        //private PolicyWrap<HttpResponseMessage> DefineAndRetrieveResiliencyStrategy()
        private AsyncPolicy<HttpResponseMessage> DefineAndRetrieveResiliencyStrategy()
        {
            // Retry when these status codes are encountered.
            HttpStatusCode[] httpStatusCodesWorthRetrying = {
                HttpStatusCode.InternalServerError, // 500
                HttpStatusCode.BadGateway, // 502
                HttpStatusCode.GatewayTimeout // 504
        };

            // Define our waitAndRetry policy: retry n times with an exponential backoff in case the Computer Vision API throttles us for too many requests.
            var waitAndRetryPolicy = Policy
                .HandleResult<HttpResponseMessage>(e => e.StatusCode == HttpStatusCode.ServiceUnavailable ||
                    e.StatusCode == (System.Net.HttpStatusCode)429)
                .WaitAndRetryAsync(10, // Retry 10 times with a delay between retries before ultimately giving up
                    attempt => TimeSpan.FromSeconds(0.25 * Math.Pow(2, attempt)), // Back off!  2, 4, 8, 16 etc times 1/4-second
                                                                                  //attempt => TimeSpan.FromSeconds(6), // Wait 6 seconds between retries
                    (exception, calculatedWaitDuration) =>
                    {
                        _logger.LogInformation($"Computer Vision API server is throttling our requests. Automatically delaying for {calculatedWaitDuration.TotalMilliseconds}ms");
                    }
                );

            // Define our first CircuitBreaker policy: Break if the action fails 4 times in a row.
            // This is designed to handle Exceptions from the Computer Vision API, as well as
            // a number of recoverable status messages, such as 500, 502, and 504.
            var circuitBreakerPolicyForRecoverable = Policy
                .Handle<HttpResponseException>()
                .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(3),
                    onBreak: (outcome, breakDelay) =>
                    {
                        _logger.LogInformation($"Polly Circuit Breaker logging: Breaking the circuit for {breakDelay.TotalMilliseconds}ms due to: {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                    },
                    onReset: () => _logger.LogInformation("Polly Circuit Breaker logging: Call ok... closed the circuit again"),
                    onHalfOpen: () => _logger.LogInformation("Polly Circuit Breaker logging: Half-open: Next call is a trial")
                );

            // Combine the waitAndRetryPolicy and circuit breaker policy into a PolicyWrap. This defines our resiliency strategy.
            return Policy.WrapAsync(waitAndRetryPolicy, circuitBreakerPolicyForRecoverable);
        }
    }
}
