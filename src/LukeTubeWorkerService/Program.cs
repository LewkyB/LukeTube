using System.Threading.RateLimiting;
using LukeTubeLib;
using LukeTubeWorkerService;
using Microsoft.Extensions.Logging.Abstractions;

var todoName = "todoPolicy";
var completeName = "completePolicy";
var helloName = "helloPolicy";
IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.AddOpenTelemetryLogging();
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddPushshiftServicesForWorker();
        services.AddOpenTelemetry();
        // services.AddRateLimiter(options =>
        // {
        //     options.AddTokenBucketLimiter(todoName, options =>
        //         {
        //             options.TokenLimit = 1;
        //             options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        //             options.QueueLimit = 1;
        //             options.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        //             options.TokensPerPeriod = 1;
        //         })
        //         .AddPolicy<string>(completeName, new RateLimitingPolicy(NullLogger<RateLimitingPolicy>.Instance))
        //         .AddPolicy<string, RateLimitingPolicy>(helloName);
        //
        //     options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        //     {
        //         return RateLimitPartition.GetConcurrencyLimiter<string>("globalLimiter", key => new ConcurrencyLimiterOptions
        //         {
        //             PermitLimit = 10,
        //             QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        //             QueueLimit = 5
        //         });
        //     });
        // });
    })
    .Build();

host.Run();