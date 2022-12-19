﻿using System.Diagnostics.Tracing;

namespace LukeTubeLib.Diagnostics;

[EventSource(Name = "PushshiftRequestCounter")]
public sealed class PushshiftRequestCounterSource : EventSource
{
    public static readonly PushshiftRequestCounterSource Log = new();

    private PollingCounter _requestCounterStarted;
    private IncrementingPollingCounter _requestCounterRateStarted;
    private long _requestCountStarted = 0;

    private PollingCounter _requestCounterFinished;
    private IncrementingPollingCounter _requestCounterRateFinished;
    private long _requestCountFinished = 0;

    private PollingCounter _requestCounterRetry;
    private IncrementingPollingCounter _requestCounterRetryRate;
    private long _requestCountRetry = 0;

    private PollingCounter _httpClientTimeoutCounter;
    private long _httpClientTimeoutCount = 0;

    private PushshiftRequestCounterSource()
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

        _requestCounterRetry = new PollingCounter(
            "request-counter-retry",
            this,
            () => Interlocked.Read(ref _requestCountRetry))
        {
            DisplayName = "Request Retry Count"
        };

        _requestCounterRetryRate = new IncrementingPollingCounter(
            "request-counter-retry-rate",
            this,
            () => Interlocked.Read(ref _requestCountRetry))
        {
            DisplayName = "Request Retry Rate",
            DisplayRateTimeScale = TimeSpan.FromMinutes(1),
        };

        _httpClientTimeoutCounter = new PollingCounter(
            "httpclient-timeout-count",
            this,
            () => Interlocked.Read(ref _httpClientTimeoutCount))
        {
            DisplayName = "Httpclient Timeout Count"
        };
    }

    public void AddStartedRequest(int count) => Interlocked.Add(ref _requestCountStarted, count);
    public void AddFinishedRequest(int count) => Interlocked.Add(ref _requestCountFinished, count);
    public void AddRetryRequest(int count) => Interlocked.Add(ref _requestCountRetry, count);
    public void AddHttpClientTimeout(int count) => Interlocked.Add(ref _httpClientTimeoutCount, count);

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

        _requestCounterRetry?.Dispose();
        _requestCounterRetry = null;

        _requestCounterRetryRate?.Dispose();
        _requestCounterRetryRate = null;

        _httpClientTimeoutCounter?.Dispose();
        _httpClientTimeoutCounter = null;

        base.Dispose(disposing);
    }
}