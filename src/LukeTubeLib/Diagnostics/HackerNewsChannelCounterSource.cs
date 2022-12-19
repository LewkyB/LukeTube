using System.Diagnostics.Tracing;

namespace LukeTubeLib.Diagnostics;

[EventSource(Name = "HackerNewsChannelCounter")]
public sealed class HackerNewsChannelCounterSource : EventSource
{
    public static readonly HackerNewsChannelCounterSource Log = new();

    private PollingCounter _channelCounter;
    private long __channelQueueCount = 0;

    private HackerNewsChannelCounterSource()
    {
        _channelCounter = new PollingCounter(
            "item-queue-count",
            this,
            () => Interlocked.Read(ref __channelQueueCount))
        {
            DisplayName = "Item Queue Count"
        };
    }

    public void AddQueueCount(int count) => Interlocked.Add(ref __channelQueueCount, count);
    public void RemoveQueueCount(int count) => Interlocked.Add(ref __channelQueueCount, -1 * count);

    protected override void Dispose(bool disposing)
    {
        _channelCounter?.Dispose();
        _channelCounter = null;

        base.Dispose(disposing);
    }
}