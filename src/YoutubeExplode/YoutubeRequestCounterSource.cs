using System;
using System.Diagnostics.Tracing;
using System.Threading;

namespace YoutubeExplode;

[EventSource(Name = "YoutubeRequestCounter")]
public sealed class YoutubeRequestCounterSource : EventSource
{
    public static readonly YoutubeRequestCounterSource Log = new();

    private PollingCounter _requestCounterStarted;
    private IncrementingPollingCounter _requestCounterRateStarted;
    private long _requestCountStarted = 0;

    private PollingCounter _requestCounterFinished;
    private IncrementingPollingCounter _requestCounterRateFinished;
    private long _requestCountFinished = 0;

    private YoutubeRequestCounterSource()
    {
        _requestCounterStarted = new PollingCounter(
            "request-counter-started",
            this,
            () => Interlocked.Read(ref _requestCountStarted))
        {
            DisplayName = "Request Started Count"
        };

        _requestCounterRateStarted = new IncrementingPollingCounter(
            "request-counter-started-rate",
            this,
            () => Interlocked.Read(ref _requestCountStarted))
        {
            DisplayName = "Request Started Rate",
            DisplayRateTimeScale = TimeSpan.FromSeconds(1),
        };

        _requestCounterStarted = new PollingCounter(
            "request-counter-finished",
            this,
            () => Interlocked.Read(ref _requestCountFinished))
        {
            DisplayName = "Request Finished Count"
        };

        _requestCounterRateStarted = new IncrementingPollingCounter(
            "request-counter-finished-rate",
            this,
            () => Interlocked.Read(ref _requestCountFinished))
        {
            DisplayName = "Request Finished Rate",
            DisplayRateTimeScale = TimeSpan.FromSeconds(1),
        };
    }

    public void AddStartedRequest(int count) => Interlocked.Add(ref _requestCountStarted, count);
    public void AddFinishedRequest(int count) => Interlocked.Add(ref _requestCountFinished, count);

    protected override void Dispose(bool disposing)
    {
        _requestCounterStarted?.Dispose();
        _requestCounterStarted = null;

        _requestCounterFinished?.Dispose();
        _requestCounterFinished = null;

        _requestCounterRateStarted?.Dispose();
        _requestCounterRateStarted = null;

        _requestCounterRateFinished?.Dispose();
        _requestCounterRateFinished = null;

        base.Dispose(disposing);
    }
}