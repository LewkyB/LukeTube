using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LukeTube.Data.Entities.PsawEntries;
using LukeTube.Data.Entities.PsawEntries.PsawSearchOptions;

namespace LukeTube.Services
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
        private readonly IHttpClientFactory _httpClientFactory;

        public PsawService(ILogger<PsawService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<T[]> Search<T>(SearchOptions options = null) where T : IEntry
        {
            var type = typeof(T).Name.Replace("Entry", "").ToLower();
            var route = string.Format(RequestsConstants.BaseAddress + RequestsConstants.SearchRoute, type);
            var result = await ExecuteGet(route, options?.ToArgs());
            return result["data"].ToObject<T[]>();
        }

        public async Task<string[]> GetSubmissionCommentIds(string base36SubmissionId)
        {
            var route = string.Format(RequestsConstants.BaseAddress + RequestsConstants.CommentIdsRoute, base36SubmissionId);
            var result = await ExecuteGet(route);
            return result["data"].ToObject<string[]>();
        }

        public async Task<MetaEntry> GetMeta()
        {
            var result = await ExecuteGet(RequestsConstants.BaseAddress + "meta");
            return result.ToObject<MetaEntry>();
        }

        // TODO: clean up exceptions and logging in ExecuteGet
        private async Task<JToken> ExecuteGet(string route, IReadOnlyCollection<string> args = null)
        {
            _logger.LogTrace($"Making HTTP request to URI: {route}");

            var httpClient = _httpClientFactory.CreateClient("PushshiftClient");
            var response = await httpClient.GetAsync(ConstructUrl(route, args));

            _logger.LogTrace($"HTTP response code: {nameof(response.StatusCode)}: {response.StatusCode}");

            // TODO: make this throw an exception on the asp.net page
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"HTTP request to {route} FAILED. Response StatusCode: {response.StatusCode}");
                throw new HttpRequestException($"request to {route} failed", null, response.StatusCode);
            }

            response.EnsureSuccessStatusCode();

            // Convert response to json
            var result = await response.Content.ReadAsStringAsync();
            return JToken.Parse(result);
        }

        private static string ConstructUrl(string route, IReadOnlyCollection<string> args) => args == null ? route : $"{route}?{ArgsToString(args)}";

        private static string ArgsToString(IEnumerable<string> args) => args.Aggregate((x, y) => $"{x}&{y}");
    }

}
