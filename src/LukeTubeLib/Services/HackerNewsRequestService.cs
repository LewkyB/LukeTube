using System.Net.Http.Json;
using LukeTubeLib.Models.HackerNews;
using Microsoft.Extensions.Logging;
using YoutubeExplode.Videos;

namespace LukeTubeLib.Services;

public interface IHackerNewsRequestService
{
    Task<IReadOnlyList<HackerNewsHit>> GetUniqueHackerNewsHits(HackerNewsSearchOptions hackerNewsSearchOption, CancellationToken cancellationToken);
    IList<HackerNewsSearchOptions> AddBeforeAndAfter(HackerNewsSearchOptions searchOption, int dayToGet);
}

// TODO: hn.algolia.com/api is rate limited to 10000/hr or 166/min, need to rework polly policy for it
public class HackerNewsRequestService : IHackerNewsRequestService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HackerNewsRequestService> _logger;

    public HackerNewsRequestService(IHttpClientFactory httpClientFactory, ILogger<HackerNewsRequestService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<HackerNewsHit>> GetUniqueHackerNewsHits(
        HackerNewsSearchOptions hackerNewsSearchOption,
        CancellationToken cancellationToken)
    {
        var hackerNewsHits = new List<HackerNewsHit>();
        var videoClient = new VideoClient(_httpClientFactory.CreateClient("YoutubeExplodeClient"));

        var rawHackerNewsHits =
            (await GetHackerNewsQueryResults<HackerNewsResponse>(
                "search",
                hackerNewsSearchOption,
                cancellationToken)).Hits.Distinct();

        // if (rawComments is null) return redditComments;

        foreach (var hit in rawHackerNewsHits)
        {
            var youtubeIds = YoutubeUtilities.FindYoutubeId(hit.story_text);
            hackerNewsHits.AddRange( CreateHackerNewsHits(youtubeIds, hit));
        }

        var uniqueHackerNewsHits = hackerNewsHits.DistinctBy(x => x.YoutubeId).ToList();
        foreach (var hackerNewsHit in uniqueHackerNewsHits)
        {
            try
            {
                var result = await videoClient.GetAsync(hackerNewsHit.YoutubeId, cancellationToken);
                if (result is not null) hackerNewsHit.VideoModel = VideoModelHelper.MapVideoEntity(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        return uniqueHackerNewsHits;
    }

    public IList<HackerNewsSearchOptions> AddBeforeAndAfter(HackerNewsSearchOptions searchOption, int dayToGet)
    {
        // 0 days ago
        //initial before
        // 24 * 0(day to get) = 0
        // initial after
        // (24 * 0(day to get)) + 2 = 2
        // add 2 to both 12 times

        // 86400 seconds in a day
        var newSearchOptions = new List<HackerNewsSearchOptions>();
        int initialBefore = 24 * 60 * 60 * dayToGet;
        int initialAfter = 24 * 60 * 60 * dayToGet + 3600;
        for (int i = 0; i < 86400; i += 3600)
        {
            initialBefore += i;
            initialAfter += i;
            string before = initialBefore + "h";
            string after = initialAfter + "h";


            var newSearchOption = new HackerNewsSearchOptions
            {
                After = after,
                HitsPerPage = searchOption.HitsPerPage,
                Before = before,
                Query = searchOption.Query,
            };

            newSearchOptions.Add(newSearchOption);
        }

        return newSearchOptions;
    }

    internal static IReadOnlyList<HackerNewsHit> CreateHackerNewsHits(IReadOnlyList<string> youtubeIds, Hit hit)
    {
        if (youtubeIds.Count <= 0){ return new List<HackerNewsHit>();}

        var hackerNewsHits = new List<HackerNewsHit>();

        foreach (var youtubeId in youtubeIds)
        {
            var redditComment = new HackerNewsHit
            {
                YoutubeId = youtubeId,
                Author = hit.author,
                Points = hit.points,
                Url = hit.url.ToString(),
            };

            hackerNewsHits.Add(redditComment);
        }

        return hackerNewsHits;
    }

    internal async Task<T> GetHackerNewsQueryResults<T>(
        string requestType,
        HackerNewsSearchOptions? searchOptions,
        CancellationToken cancellationToken) where T : new()
    {
        var hackerNewsUrl = requestType switch
        {
            "search" => $"search?{YoutubeUtilities.ArgsToString(searchOptions.ToArgs())}",
            _ => ""
        };

        // TODO: why isnt the base address changing from push shift here?
        const string hackerNewsBaseAddress = "https://hn.algolia.com/api/v1/";
        var httpClient = _httpClientFactory.CreateClient("HackerNewsRequestServiceClient");
        httpClient.BaseAddress = new Uri(hackerNewsBaseAddress);

        var result = new T();

            try
        {
            result = await httpClient.GetFromJsonAsync<T>(hackerNewsUrl, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        return result;
    }
}