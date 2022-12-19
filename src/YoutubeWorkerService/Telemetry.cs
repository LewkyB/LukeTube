using System.Diagnostics;

namespace LukeTubeWorkerService;

public static class Telemetry
{
    public const string ServiceName = "YoutubeServiceWorker";
    public const string ServiceVersion = "1.0.0";
    public static readonly ActivitySource MyActivitySource = new(ServiceName, ServiceVersion);
}