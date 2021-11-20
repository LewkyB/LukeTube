using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PsawSharp.Entries;
using PsawSharp.Requests.Options;
using RateLimiter;

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
        private readonly TimeLimiter _timeLimiter;
        private readonly ILogger<PsawService> _logger;
        private readonly HttpClient _httpClient;

        public PsawService(HttpClient httpClient, ILogger<PsawService> logger)
        {
            _timeLimiter = TimeLimiter.GetFromMaxCountByInterval(RequestsConstants.MaxRequestsPerMinute, TimeSpan.FromSeconds(60));
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<T[]> Search<T>(SearchOptions options = null) where T : IEntry
        {
            string type = typeof(T).Name.Replace("Entry", "").ToLower();
            string route = string.Format(RequestsConstants.SearchRoute, type);
            var result = await PerformGet(RequestsConstants.BaseAddress + route, options?.ToArgs());
            return result["data"].ToObject<T[]>();
        }

        public async Task<string[]> GetSubmissionCommentIds(string base36SubmissionId)
        {
            string route = string.Format(RequestsConstants.BaseAddress + RequestsConstants.CommentIdsRoute, base36SubmissionId);
            var result = await PerformGet(route);
            return result["data"].ToObject<string[]>();
        }

        public async Task<MetaEntry> GetMeta()
        {
            var result = await PerformGet(RequestsConstants.BaseAddress + "meta");
            return result.ToObject<MetaEntry>();
        }

        internal async Task<JToken> PerformGet(string route, List<string> args = null)
        {
            return await _timeLimiter.Perform(() => ExecuteGet(route, args));
        }

        private async Task<JToken> ExecuteGet(string route, List<string> args = null)
        {
            _logger.LogTrace($"Executing GET request to {nameof(route)} {route}");

            // Execute request and ensure it didn't fail
            var response = await _httpClient.GetAsync(ConstructUrl(route, args));

            _logger.LogTrace($"response HTTP {nameof(response.StatusCode)} {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PsawService.ExecuteGet Failure");
                throw new HttpRequestException($"request to {route} failed", null, response.StatusCode);
            }

            response.EnsureSuccessStatusCode();

            // Convert response to json
            string result = await response.Content.ReadAsStringAsync();
            return JToken.Parse(result);
        }

        private string ConstructUrl(string route, List<string> args) => args == null ? route : $"{route}?{ArgsToString(args)}";

        private static string ArgsToString(List<string> args) => args.Aggregate((x, y) => $"{x}&{y}");
    }
}
