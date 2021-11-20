using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PsawSharp.Requests.Options;
using RateLimiter;

namespace PsawSharp.Requests
{
    public class RequestsManager
    {

        #region Fields

        private readonly RequestsManagerOptions _options;
        private readonly TimeLimiter _timeLimiter;
        private HttpClient _httpClient;

        #endregion

        public RequestsManager()
        {
            _options = new RequestsManagerOptions();
            _timeLimiter = TimeLimiter.GetFromMaxCountByInterval(RequestsConstants.MaxRequestsPerMinute, TimeSpan.FromSeconds(60));
            InitializeHttpClient();
        }

        public RequestsManager(RequestsManagerOptions options)
        {
            _options = options;
            _timeLimiter = TimeLimiter.GetFromMaxCountByInterval(RequestsConstants.MaxRequestsPerMinute, TimeSpan.FromSeconds(60));
            InitializeHttpClient();
        }


        internal async Task<JToken> PerformGet(string route, List<string> args = null)
        {
            return await _timeLimiter.Perform(() => ExecuteGet(route, args));
        }


        private async Task<JToken> ExecuteGet(string route, List<string> args = null)
        {
            // Execute request and ensure it didn't fail
            var response = await _httpClient.GetAsync(ConstructUrl(route, args));
            response.EnsureSuccessStatusCode();

            // Convert response to json
            string result = await response.Content.ReadAsStringAsync();
            return JToken.Parse(result);
        }

        private string ConstructUrl(string route, List<string> args) => args == null ? route : $"{route}?{ArgsToString(args)}";

        private static string ArgsToString(List<string> args) => args.Aggregate((x, y) => $"{x}&{y}");

        private void InitializeHttpClient()
        {
            if (_options.UseProxy)
            {
                _httpClient = new HttpClient(new HttpClientHandler
                {
                    UseProxy = true,
                    Proxy = new WebProxy(_options.ProxyAddress)
                });
            }
            else
            {
                _httpClient = new HttpClient();
            }

            _httpClient.BaseAddress = new Uri(RequestsConstants.BaseAddress);
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", _options.UserAgent);
        }
    }
}
