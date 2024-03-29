﻿using System.Text;
using LukeTube;
using LukeTube.BackgroundServices;
using LukeTube.Data;
using LukeTube.PollyPolicies;
using LukeTube.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

const string allowSpecificOrigins = "_allowSpecificOrigins";
const string pushshiftBaseAddress = "https://api.pushshift.io/";

builder.Services.AddHttpClient<IPushshiftRequestService, PushshiftRequestService>("PushshiftServiceClient", client =>
    {
        client.BaseAddress = new Uri(pushshiftBaseAddress);
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(2))
    .AddPolicyHandler(PushshiftPolicies.GetWaitAndRetryPolicy())
    .AddPolicyHandler(PushshiftPolicies.GetRateLimitPolicy());

builder.Services.AddScoped<IPushshiftRepository, PushshiftRepository>();
builder.Services.AddScoped<IPushshiftService, PushshiftService>();
builder.Services.AddScoped<IPushshiftRequestService, PushshiftRequestService>();

builder.Services.AddHostedService<PushshiftBackgroundService>();

builder.Services.AddControllers();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = Environment.GetEnvironmentVariable("CONNECTION_STRINGS__REDIS");
    options.InstanceName = "LukeTube_";
});

builder.Services.AddDbContextPool<PushshiftContext>(options =>
{
    options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRINGS__POSTGRESQL"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:81/")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin();
        });
});

builder.Services.AddOpenTelemetryTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder
        .AddSource(Telemetry.ServiceName)
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName: Telemetry.ServiceName, serviceVersion: Telemetry.ServiceVersion)
        )
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation(o =>
        {
            o.Filter = httpContext => !httpContext.Request.Path.ToString().Contains("/_");
            o.Enrich = (activity, eventName, rawObject) =>
            {
                switch (eventName)
                {
                    case "OnStartActivity":
                    {
                        if (rawObject is HttpRequest httpRequest)
                        {
                            activity.SetStartTime(DateTime.Now);
                            activity.SetTag("http.method", httpRequest.Method);
                            activity.SetTag("http.url", httpRequest.Path);
                        }

                        break;
                    }
                    case "OnStopActivity":
                    {
                        if (rawObject is HttpResponse httpResponse)
                        {
                            activity.SetEndTime(DateTime.Now);
                            activity.SetTag("responseLength", httpResponse.ContentLength);
                        }

                        break;
                    }
                }
            };
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

builder.Services.AddOpenTelemetryMetrics(options =>
{
    options
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Telemetry.ServiceName, Telemetry.ServiceVersion).AddTelemetrySdk())
        .AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri("http://otel_collector:4317");
            o.Protocol = OtlpExportProtocol.Grpc;
        });
});
builder.Services.AddSingleton(TracerProvider.Default.GetTracer(Telemetry.ServiceName));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Logging.AddOpenTelemetry(options =>
{
    options.ConfigureResource(r =>
    {
        r.AddService(Telemetry.ServiceName, Telemetry.ServiceVersion);
    });
    options.AddOtlpExporter(o =>
    {
        o.Endpoint = new Uri("http://otel_collector:4317");
    });

    options.IncludeScopes = true;
    options.IncludeFormattedMessage = true;
    options.ParseStateValues = true;
});

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var currentTimeUtc = DateTime.UtcNow.ToString();

    var encodedCurrentTimeUtc = Encoding.UTF8.GetBytes(currentTimeUtc);

    var options = new DistributedCacheEntryOptions()
        .SetSlidingExpiration(TimeSpan.FromSeconds(20));

    app.Services.GetService<IDistributedCache>()?.Set("cachedTimeUTC", encodedCurrentTimeUtc, options);
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseStatusCodePages();
    app.UseCors(allowSpecificOrigins);
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

    // not sure if this is needed since nginx is handling https
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseResponseCaching();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();