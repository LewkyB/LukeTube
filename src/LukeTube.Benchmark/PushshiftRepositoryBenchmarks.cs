using BenchmarkDotNet.Attributes;
using LukeTubeLib.Models.Pushshift;
using LukeTubeLib.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace LukeTube.Benchmark;

[MemoryDiagnoser]
public class PushshiftRepositoryBenchmarks
{

    private PushshiftRepository _pushshiftRepository;
    [GlobalSetup]
    public void GlobalSetup()
    {
        DbContextOptionsBuilder<PushshiftContext> dbOption = new DbContextOptionsBuilder<PushshiftContext>()
            .UseNpgsql("host=localhost;database=SubredditDb;username=postgres;password=postgres;")
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging();

        var pushshiftContext = new PushshiftContext(dbOption.Options);

        // Random rnd = new Random();
        //
        // pushshiftContext.RedditComments.AddRange(Enumerable.Range(0, 1000).Select(i => new RedditComment
        // {
        //     Subreddit = AllSubreddits.Subreddits[rnd.Next(0, AllSubreddits.Subreddits.Count)],
        //     VideoModel = null,
        //     Permalink = "",
        //     Score = 1,
        //     YoutubeLinkId = "lfkd56gyke4",
        //     CreatedUTC = DateTime.UtcNow,
        //     RetrievedUTC = DateTime.UtcNow,
        //     RedditCommentId = 0,
        // }));
        //
        // pushshiftContext.RedditComments.AddRange(Enumerable.Range(0, 100).Select(i => new RedditComment
        // {
        //     Subreddit = "aviation",
        //     VideoModel = null,
        //     Permalink = "",
        //     Score = 1,
        //     YoutubeLinkId = "lfkd56gyke4",
        //     CreatedUTC = DateTime.UtcNow,
        //     RetrievedUTC = DateTime.UtcNow,
        //     RedditCommentId = 0,
        // }));
        // pushshiftContext.SaveChanges();

        _pushshiftRepository = new PushshiftRepository(pushshiftContext, NullLogger<PushshiftRepository>.Instance);
    }

    [Benchmark]
    public Task<IReadOnlyList<RedditComment>> GetAllComments()
    {
        return _pushshiftRepository.GetAllRedditComments();
    }

    [Benchmark]
    public Task<IReadOnlyList<string>> GetAllSubredditNames()
    {
        return _pushshiftRepository.GetAllSubredditNames();
    }

    [Benchmark]
    public Task<IReadOnlyList<RedditComment>> GetCommentsForSubreddit()
    {
        return _pushshiftRepository.GetCommentsBySubreddit("aviation");
    }
}