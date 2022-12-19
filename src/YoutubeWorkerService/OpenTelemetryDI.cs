﻿using LukeTubeWorkerService;
using Npgsql;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace YoutubeWorkerService;

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
                // .AddMassTransitInstrumentation()
                .AddNpgsql()
                .AddEntityFrameworkCoreInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                    options.SetDbStatementForStoredProcedure = true;
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
                .AddProcessInstrumentation()
                .AddRuntimeInstrumentation()
                .AddEventCountersInstrumentation(options =>
                {
                    options.RefreshIntervalSecs = 1;
                    options.AddEventSources(
                        "System.Net.Http",
                        "System.Net.Sockets",
                        "System.Net.NameResolution",
                        "System.Net.Security",
                        "PushshiftChannelCounter",
                        "HackerNewsChannelCounter",
                        "YoutubeRequestCounter"
                    );
                })
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Telemetry.ServiceName, Telemetry.ServiceVersion).AddTelemetrySdk())
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