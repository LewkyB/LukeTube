using LukeTube.Services;

namespace LukeTube.BackgroundServices
{
    public sealed class PushshiftBackgroundService : BackgroundService
    {
        private readonly ILogger<PushshiftBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public PushshiftBackgroundService(ILogger<PushshiftBackgroundService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        // background task that is constantly running and loading the database
        // with data from Pushshift's API
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(PushshiftBackgroundService)} is running.");

            await DoWorkAsync(cancellationToken);
        }

        private async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"{nameof(PushshiftBackgroundService)} is working.");

                // in order to use a scoped service in a background service you must create it's own scope
                using var scope = _serviceProvider.CreateScope();
                var pushshiftService = scope.ServiceProvider.GetService<IPushshiftRequestService>();

                if (pushshiftService is null) throw new NullReferenceException(nameof(pushshiftService));

                var subreddits = await pushshiftService.GetSubreddits();
                await pushshiftService.GetUniqueRedditComments(subreddits, 365, 100);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Pushshift API unavailable {ex.StatusCode}");
            }
            finally
            {
                // await StopAsync(CancellationToken.None);
                await StopAsync(cancellationToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(PushshiftBackgroundService)} is stopping.");

            await base.StopAsync(cancellationToken);
        }
    }
}