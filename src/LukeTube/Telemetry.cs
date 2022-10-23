using System.Diagnostics;

namespace LukeTube;

public static class Telemetry
{
    public const string ServiceName = "LukeTube.Backend";
    public const string ServiceVersion = "1.0.0";
    public static readonly ActivitySource MyActivitySource = new(ServiceName, ServiceVersion);
}