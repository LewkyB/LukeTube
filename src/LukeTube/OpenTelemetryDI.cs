using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;

namespace LukeTube;

public static class OpenTelemetryDI
{
    public static IServiceCollection AddOpenTelemetry(this IServiceCollection services)
    {
        services.AddOpenTelemetryTracing(tracerProviderBuilder =>
        {
            tracerProviderBuilder
                .AddSource(Telemetry.ServiceName)
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName: Telemetry.ServiceName, serviceVersion: Telemetry.ServiceVersion)
                )
                .AddHttpClientInstrumentation((options) =>
                {
                    // Note: Only called on .NET & .NET Core runtimes.
                    options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
                    {
                        activity.SetTag("requestVersion", httpRequestMessage.Version);
                    };
                    // Note: Only called on .NET & .NET Core runtimes.
                    options.EnrichWithHttpResponseMessage = (activity, httpResponseMessage) =>
                    {
                        activity.SetTag("responseVersion", httpResponseMessage.Version);
                    };
                    // Note: Called for all runtimes.
                    options.EnrichWithException = (activity, exception) =>
                    {
                        activity.SetTag("stackTrace", exception.StackTrace);
                    };
                    options.RecordException = true;
                })
                .AddAspNetCoreInstrumentation(o =>
                {
                    // o.Filter = httpContext => !httpContext.Request.Path.ToString().Contains("/_");
                    o.EnrichWithHttpRequest = (activity, httpRequest) =>
                    {
                        activity.SetTag("requestProtocol", httpRequest.Protocol);
                    };
                    o.EnrichWithHttpResponse = (activity, httpResponse) =>
                    {
                        activity.SetTag("responseLength", httpResponse.ContentLength);
                    };
                    o.EnrichWithException = (activity, exception) =>
                    {
                        activity.SetTag("exceptionType", exception.GetType().ToString());
                    };
                    o.RecordException = true;
                })
                .AddRedisInstrumentation(ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("CONNECTION_STRINGS__REDIS")), options =>
                {
                    options.FlushInterval = TimeSpan.FromSeconds(5);
                    options.SetVerboseDatabaseStatements = true;
                    options.EnrichActivityWithTimingEvents = true;
                    options.Enrich = (activity, command) =>
                    {
                        if (command.ElapsedTime < TimeSpan.FromMilliseconds(100))
                        {
                            activity.SetTag("is_fast", true);
                        }
                    };
                })
                .AddEntityFrameworkCoreInstrumentation(o =>
                {
                    o.SetDbStatementForText = true;
                    o.SetDbStatementForStoredProcedure = true;
                })
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri("http://otel_collector:4317");
                    o.Protocol = OtlpExportProtocol.Grpc;
                });
        });

        services.AddOpenTelemetryMetrics(options =>
        {
            options
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddProcessInstrumentation()
                .AddRuntimeInstrumentation()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Telemetry.ServiceName, Telemetry.ServiceVersion).AddTelemetrySdk())
                .AddEventCountersInstrumentation(options =>
                {
                    options.RefreshIntervalSecs = 1;
                    // TODO: anymore event sources I could add/remove here?
                    options.AddEventSources(
                        "System.Net.Http",
                        "System.Net.Sockets",
                        "System.Net.NameResolution",
                        "System.Net.Security",
                        "Microsoft.EntityFrameworkCore"
                    );
                })
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri("http://otel_collector:4317");
                    o.Protocol = OtlpExportProtocol.Grpc;
                });
        });

        services.AddSingleton(TracerProvider.Default.GetTracer(Telemetry.ServiceName));

        return services;
    }

    public static ILoggingBuilder AddOpenTelemetryLogging(this ILoggingBuilder logging)
    {
        logging.AddOpenTelemetry(options =>
        {
            options.ConfigureResource(r =>
            {
                r.AddService(Telemetry.ServiceName, Telemetry.ServiceVersion);
            });
            options.AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri("http://otel_collector:4317");
            });
            options.AttachLogsToActivityEvent();
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
            options.ParseStateValues = true;
        });

        return logging;
    }
}