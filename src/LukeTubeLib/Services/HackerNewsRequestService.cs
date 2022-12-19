using System.Net;
using System.Text.Json;
using LukeTubeLib.Diagnostics;
using LukeTubeLib.Models.HackerNews;
using Microsoft.Extensions.Logging;
using Polly.RateLimit;
using YoutubeExplode.Exceptions;

namespace LukeTubeLib.Services;

public interface IHackerNewsRequestService
{
    IList<HackerNewsSearchOptions> AddBeforeAndAfter(HackerNewsSearchOptions searchOption, int dayToGet);
    Task<IReadOnlyList<HackerNewsMessage>> GetHackerNewsHits(HackerNewsSearchOptions hackerNewsSearchOption, CancellationToken cancellationToken);
}

public class HackerNewsRequestService : IHackerNewsRequestService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HackerNewsRequestService> _logger;

    public HackerNewsRequestService(ILogger<HackerNewsRequestService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<HackerNewsMessage>> GetHackerNewsHits(
        HackerNewsSearchOptions hackerNewsSearchOption,
        CancellationToken cancellationToken)
    {
        var hackerNewsMessages = new List<HackerNewsMessage>();

        var hackerNewsHits = new List<Hit>();
        // try
        // {
            var result  = await GetHackerNewsQueryResults("search", hackerNewsSearchOption, cancellationToken)
                .ConfigureAwait(false);

            if (!(result.Hits.Count > 0)) return hackerNewsMessages;

            hackerNewsHits.AddRange(result.Hits);

            if (result.NbPages > 0)
            {
                for (int i = 1; i < result.NbPages; i++)
                {
                    hackerNewsSearchOption.Page = i;
                    var results = await GetHackerNewsQueryResults("search", hackerNewsSearchOption, cancellationToken).ConfigureAwait(false);
                    hackerNewsHits.AddRange(results.Hits);
                }
            }
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError(ex, ex.Message);
        // }


        foreach (var hit in hackerNewsHits)
        {
            var youtubeIds = YoutubeUtilities.FindYoutubeId(hit.CommentText);
            hackerNewsMessages.AddRange( CreateHackerNewsHits(youtubeIds, hit));
        }

        return hackerNewsMessages.DistinctBy(x => x.YoutubeId).ToList();
    }

    public IList<HackerNewsSearchOptions> AddBeforeAndAfter(HackerNewsSearchOptions searchOption, int dayToGet)
    {
        var newSearchOptions = new List<HackerNewsSearchOptions>();

        var initialBefore = DateTimeOffset.Now.ToUnixTimeSeconds() - 86400;
        var initialAfter = DateTimeOffset.Now.ToUnixTimeSeconds();

        for (int i = 0; i < 604800; i += 86400)
        {
            initialBefore -= i;
            initialAfter -= i;
            var before = initialBefore;
            var after = initialAfter;

            var newSearchOption = new HackerNewsSearchOptions
            {
                After = after.ToString(),
                HitsPerPage = searchOption.HitsPerPage,
                Before = before.ToString(),
                Query = searchOption.Query,
                Tags = searchOption.Tags,
            };

            newSearchOptions.Add(newSearchOption);
        }

        return newSearchOptions;
    }

    internal static IReadOnlyList<HackerNewsMessage> CreateHackerNewsHits(IReadOnlyList<string> youtubeIds, Hit hit)
    {
        if (youtubeIds.Count <= 0) return new List<HackerNewsMessage>();

        return youtubeIds.Select(youtubeId => new HackerNewsMessage
        {
            YoutubeId = youtubeId,
            Author = hit.Author,
            Points = hit.Points,
            Url = $"https://news.ycombinator.com/item?id={hit.ParentId}",
        }).ToList();
    }

    internal async Task<HackerNewsResponse> GetHackerNewsQueryResults(
        string requestType,
        HackerNewsSearchOptions? searchOptions,
        CancellationToken cancellationToken)
    {
        var hackerNewsUrl = requestType switch
        {
            "search" => $"search_by_date?{YoutubeUtilities.ArgsToString(searchOptions.ToArgs())}",
            // "search" => $"search?{YoutubeUtilities.ArgsToString(searchOptions.ToArgs())}",
            _ => ""
        };

        var result = new HackerNewsResponse();

        HackerNewsRequestCounterSource.Log.AddStartedRequest(1);
        try
        {
            var response = await _httpClient.GetAsync(hackerNewsUrl, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode is not HttpStatusCode.OK) return result;

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            result = await JsonSerializer.DeserializeAsync<HackerNewsResponse>(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
            HackerNewsRequestCounterSource.Log.AddFinishedRequest(1);
        }
        catch (RateLimitRejectedException) { }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, ex.Message);
        }


        return result;
    }
}