using System.IO.Compression;
using System.Text;
using LukeTube;
using LukeTube.Services;
using LukeTubeLib.Repositories;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

// TODO: https://www.reddit.com/r/dotnet/comments/yrq8g7/net6_webapi_environment_variables_how_to_publish/
// TODO: https://www.reddit.com/r/dotnet/comments/yrf54w/net6_how_to_allow_origins_for_cors_correctly_for/ (might not need to worry about cors stuff with k8 ingress?)
// TODO: how to fix the DI errors that occur when you disable caching in the .env file? possible with constructor DI?
// TODO: entities saving to database that use c#'s DateTimeOffset are storing only a value of infinite
// TODO: how to use dockerfiles with library dependencies from within their project folder, it's messy having them outside, should i somehow use the solution file?
// TODO:

// APIs to add
// TODO: https://github.com/4chan/4chan-API
// TODO: https://hn.algolia.com/api
// TODO: get submissions from pushshift
// TODO:

// TODO: add front end to containers to run in container tests
// TODO: add playwright tests
// TODO:

// features
// TODO: with more data from the above youtube library, this might get enough extra data to use a recommendation engine for users (how to keep track of users? cookies or?)
// TODO: can i use something like elastic search to index stuff any get rid of long duration queries?
// TODO: is there a way to use benchmarkdotnet for tracking and alerting to performance changes? (use EnvironmentAnalyser from benchmarkdotnet if using it in containers)
// TODO: look through these https://github.com/search?q=youtube+language%3AC%23&type=repositories&l=C%23&s=stars&o=desc
// TODO:

var builder = WebApplication.CreateBuilder(args);

const string allowSpecificOrigins = "_allowSpecificOrigins";

builder.Services.AddControllers();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = Environment.GetEnvironmentVariable("CONNECTION_STRINGS__REDIS");
    options.InstanceName = "LukeTube_";
});

builder.Services.AddScoped<IPushshiftRepository, PushshiftRepository>();
builder.Services.AddDbContextPool<PushshiftContext>(options =>
{
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
    options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRINGS__POSTGRESQL_PUSHSHIFT"));
});

builder.Services.AddScoped<IPushshiftService, PushshiftService>();

// add tracing and metrics
builder.Services.AddOpenTelemetry();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
// builder.Services.AddOutputCache();
builder.Services.AddResponseCompression(options =>
{
    // TODO: is enabling for HTTPS a security issue for me?
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.SmallestSize;
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

builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Logging.AddOpenTelemetryLogging();

// I probably don't need both of these
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var currentTimeUtc = DateTime.UtcNow.ToString();
    var encodedCurrentTimeUtc = Encoding.UTF8.GetBytes(currentTimeUtc);
    var options = new DistributedCacheEntryOptions()
        .SetSlidingExpiration(TimeSpan.FromSeconds(30));
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

    // not sure if this is needed since nginx/ingress is handling https
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
}

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseRouting();

// must be called after UseCors
// app.UseOutputCache();

app.UseAuthorization();
app.MapControllers();

app.Run();