﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LukeTube.Services.BackgroundServices
{
    public class PushshiftBackgroundService : BackgroundService
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
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{nameof(PushshiftBackgroundService)} is running.");

            await DoWorkAsync(stoppingToken);
        }

        private async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(PushshiftBackgroundService)} is working.");

            // in order to use a scoped service in a background service you must create it's own scope
            using var scope = _serviceProvider.CreateScope();
            var pushshiftService = scope.ServiceProvider.GetService<IPushshiftService>();

            if (pushshiftService != null) await pushshiftService.GetLinksFromCommentsAsync();
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{nameof(PushshiftBackgroundService)} is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}
