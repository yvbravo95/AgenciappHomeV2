using Agenciapp.Service.IReporteCobroServices;
using Agenciapp.Service.PassportServices;
using Agenciapp.Service.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Agenciapp.Service
{
    public class WorkerBackground : IHostedService
    {
        private readonly ILogger<WorkerBackground> _logger;
        private readonly IServiceScopeFactory scopeFactory;
        public WorkerBackground(ILogger<WorkerBackground> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            this.scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var date = DateTime.Now;
                var scope = scopeFactory.CreateScope();
                /*if (date.Hour == 23 && date.Minute == 00) 
                {
                    var taskReportCobro = scope.ServiceProvider.GetRequiredService<IReporteCobroService>();
                    var task1 = taskReportCobro.CreateReport();
                    _logger.LogInformation("Created Report Cobro at: {time}", DateTimeOffset.Now);
                }*/
                /*if ((date.Hour == 9 && date.Minute == 00) || (date.Hour == 15 && date.Minute == 00)) 
                {
                    var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
                    var task2 = taskService.AlertFinishDueDate();
                    _logger.LogInformation("Task Service running at: {time}", DateTimeOffset.Now);
                }*/
                if(date.Hour == 22 && date.Minute == 00){
                    var taskService = scope.ServiceProvider.GetRequiredService<IPassportService>();
                    _ = taskService.SendEmailReviewDCuba();
                    _logger.LogInformation("Send Emails Review DCuba at: {time}", DateTimeOffset.Now);
                }

                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping my service...");
            return Task.CompletedTask;
        }
    }
}
