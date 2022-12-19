using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using LukeTubeLib.Diagnostics;
using LukeTubeLib.Models.Pushshift;
using Microsoft.Extensions.Logging;
using Polly.RateLimit;
using YoutubeExplode.Exceptions;

namespace LukeTubeLib.Services
{
    public interface IPushshiftRequestService
    {
        List<PushshiftSearchOptions> BuildSearchOptionsNoDates(string query, int maxNumComments);
        IList<PushshiftSearchOptions> AddBeforeAndAfter(IList<PushshiftSearchOptions> searchOptions, int dayToGet);
        Task<IReadOnlyList<PushshiftMessage>> GetRedditComments(PushshiftSearchOptions pushshiftSearchOption, CancellationToken cancellationToken);
    }
    public sealed class PushshiftRequestService : IPushshiftRequestService
    {
        private readonly ILogger<PushshiftRequestService> _logger;
        private readonly HttpClient _httpClient;

        public PushshiftRequestService(
            ILogger<PushshiftRequestService> logger,
            HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<PushshiftMessage>> GetRedditComments(
            PushshiftSearchOptions pushshiftSearchOption,
            CancellationToken cancellationToken)
        {
            var pushshiftMessages = new List<PushshiftMessage>();

            IReadOnlyList<PushshiftCommentResponse> rawComments = null;
            // try
            // {
            rawComments = (await GetPushshiftQueryResults<CommentResponse>(
                "comment", pushshiftSearchOption, cancellationToken).ConfigureAwait(false) ).Data;
            // }
            // catch (Exception ex)
            // {
            //     _logger.LogError(ex, ex.Message);
            // }

            if (!(rawComments?.Count > 0)) return pushshiftMessages;

            foreach (var comment in rawComments.Distinct())
            {
                var youtubeIds = YoutubeUtilities.FindYoutubeId(comment.Body);
                pushshiftMessages.AddRange( CreatePushshiftMessages(youtubeIds, comment));
            }

            var uniqueRedditComments = pushshiftMessages.DistinctBy(x => x.YoutubeId).ToList();

            return uniqueRedditComments;
        }

        public List<PushshiftSearchOptions> BuildSearchOptionsNoDates(string query, int maxNumComments)
        {
            var searchOptions = new List<PushshiftSearchOptions>();
            var stringBuilder = new StringBuilder();
            var counter = 0;

            // < or <=, am I cutting off the last subreddit?
            while (counter < AllSubreddits.Subreddits.Count)
            {
                var searchOption = new PushshiftSearchOptions
                {
                    Subreddit = string.Empty,
                    Query = query,
                    Before = null,
                    After = null,
                    Size = maxNumComments
                };

                stringBuilder.Append(AllSubreddits.Subreddits[counter++]);

                // get the total length of the search options that will get appended to the URI
                var searchOptionLength = searchOption.ToString().Length;

                // 1950 because we're aiming for a max length of 2000, we want to leave plenty of wiggle room
                // because there is still the URI to consider
                while (stringBuilder.Length < 1950 - searchOptionLength && counter < AllSubreddits.Subreddits.Count)
                {
                    // TODO: better way that interpolation?
                    stringBuilder.Append($",{AllSubreddits.Subreddits[counter++]}");
                }

                searchOption.Subreddit = stringBuilder.ToString();
                stringBuilder.Clear();
                searchOptions.Add(searchOption);
            }

            return searchOptions;
        }

        public IList<PushshiftSearchOptions> AddBeforeAndAfter(IList<PushshiftSearchOptions> searchOptions, int dayToGet)
        {
            var newSearchOptions = new List<PushshiftSearchOptions>();

            int initialBefore = 24 * dayToGet;
            int initialAfter = 24 * dayToGet + 2;

            for (int i = 0; i < 24; i += 2)
            {
                initialBefore += i;
                initialAfter += i;
                string before = initialBefore + "h";
                string after = initialAfter + "h";

                newSearchOptions.AddRange(searchOptions.Select(t => new PushshiftSearchOptions
                {
                    Subreddit = t.Subreddit,
                    After = after,
                    Size = t.Size,
                    Before = before,
                    Query = t.Query,
                }));
            }

            return newSearchOptions;
        }

        internal List<PushshiftSearchOptions> BuildSearchOptions(string query, int maxNumComments, string before, string after)
        {
            var searchOptions = new List<PushshiftSearchOptions>();
            var stringBuilder = new StringBuilder();
            var counter = 0;

            // < or <=, am I cutting off the last subreddit?
            while (counter < AllSubreddits.Subreddits.Count)
            {
                var searchOption = new PushshiftSearchOptions
                {
                    Subreddit = string.Empty,
                    Query = query,
                    Before = before,
                    After = after,
                    Size = maxNumComments
                };

                stringBuilder.Append(AllSubreddits.Subreddits[counter++]);

                // get the total length of the search options that will get appended to the URI
                var searchOptionLength = searchOption.ToString().Length;

                // 1950 because we're aiming for a max length of 2000, we want to leave plenty of wiggle room
                // because there is still the URI to consider
                while (stringBuilder.Length < 1950 - searchOptionLength && counter < AllSubreddits.Subreddits.Count)
                {
                    // TODO: better way that interpolation?
                    stringBuilder.Append($",{AllSubreddits.Subreddits[counter++]}");
                }

                searchOption.Subreddit = stringBuilder.ToString();
                stringBuilder.Clear();
                searchOptions.Add(searchOption);
            }

            return searchOptions;
        }

        internal static IReadOnlyList<PushshiftMessage> CreatePushshiftMessages(IReadOnlyList<string> youtubeIds, PushshiftCommentResponse comment)
        {
            if (youtubeIds.Count <= 0) return new List<PushshiftMessage>();

            return youtubeIds.Select(youtubeId => new PushshiftMessage
            {
                Subreddit = comment.Subreddit,
                YoutubeId = youtubeId,
                CreatedUTC = comment.CreatedUtc,
                Score = comment.Score,
                RetrievedUTC = comment.RetrievedOn,
                Permalink = comment.Permalink
            }).ToList();
        }

        internal async Task<T> GetPushshiftQueryResults<T>(
            string requestType,
            PushshiftSearchOptions? searchOptions,
            CancellationToken cancellationToken) where T : new()
        {
            var pushshiftUrl = requestType switch
            {
                "comment" => $"reddit/comment/search?{YoutubeUtilities.ArgsToString(searchOptions.ToArgs())}",
                "submission" => $"reddit/submission/search?{YoutubeUtilities.ArgsToString(searchOptions.ToArgs())}",
                "meta" => "meta",
                _ => ""
            };

            var result = new T();

            PushshiftRequestCounterSource.Log.AddStartedRequest(1);

            try
            {
                var response = await _httpClient.GetAsync(pushshiftUrl, cancellationToken).ConfigureAwait(false);

                // if (response.StatusCode is not HttpStatusCode.OK) return result;

                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                result = await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
                PushshiftRequestCounterSource.Log.AddFinishedRequest(1);
            }
            catch (RateLimitRejectedException)
            {
            }
            catch (TaskCanceledException ex)
            {
                string timeoutMessage = $"The request was canceled due to the configured HttpClient.Timeout of {_httpClient.Timeout.TotalSeconds} seconds elapsing.";
                if (ex.CancellationToken.IsCancellationRequested && ex.Message.Equals(timeoutMessage))
                {
                    PushshiftRequestCounterSource.Log.AddHttpClientTimeout(1);
                    _logger.LogTrace(ex, ex.Message);
                }
                else { throw ex; }
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex, "An unhandled exception occurred.");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "An unhandled exception occurred.");
            }

            return result;
        }
    }
}