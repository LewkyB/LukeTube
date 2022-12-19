using System.Diagnostics.Tracing;

namespace LukeTubeLib.Diagnostics;

[EventSource(Name = "PushshiftChannelCounter")]
public sealed class PushshiftChannelCounterSource : EventSource
{
    public static readonly PushshiftChannelCounterSource Log = new();

    private PollingCounter _channelCounter;
    private long __channelQueueCount = 0;

    private PushshiftChannelCounterSource()
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