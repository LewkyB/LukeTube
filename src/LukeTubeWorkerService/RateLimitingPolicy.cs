// using System.Threading.RateLimiting;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.RateLimiting;
//
// namespace LukeTubeWorkerService;
//
// public class RateLimitingPolicy : IRateLimiterPolicy<string>
// {
//     private Func<OnRejectedContext, CancellationToken, ValueTask>? _onRejected;
//
//     public RateLimitingPolicy(ILogger<RateLimitingPolicy> logger)
//     {
//         _onRejected = (context, token) =>
//         {
//             context.HttpContext.Response.StatusCode = 429;
//             logger.LogInformation($"Request rejected by {nameof(RateLimitingPolicy)}");
//             return ValueTask.CompletedTask;
//         };
//     }
//
//     public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get => _onRejected; }
//
//     // Use a sliding window limiter allowing 1 request every 10 seconds
//     public RateLimitPartition<string> GetPartition(HttpContext httpContext)
//     {
//         return RateLimitPartition.GetSlidingWindowLimiter<string>(string.Empty, key => new SlidingWindowRateLimiterOptions
//         {
//             PermitLimit = 1,
//             QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
//             QueueLimit = 1,
//             Window = TimeSpan.FromSeconds(5),
//             SegmentsPerWindow = 1
//         });
//     }
// }