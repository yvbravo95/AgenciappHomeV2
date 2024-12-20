using AgenciappHome.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Agenciapp.Service.Tasks
{
    public class WorkerTask : IHostedService
    {
        private readonly ILogger<WorkerTask> _logger;
        private readonly IServiceScopeFactory scopeFactory;
        public WorkerTask(ILogger<WorkerTask> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            this.scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    

                }
                await Task.Delay(TimeSpan.FromHours(2), cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping my service...");
            return Task.CompletedTask;
        }
    }
}
