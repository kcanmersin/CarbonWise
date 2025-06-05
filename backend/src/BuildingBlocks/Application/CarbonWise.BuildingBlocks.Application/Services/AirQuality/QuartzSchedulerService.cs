// backend/src/BuildingBlocks/Application/CarbonWise.BuildingBlocks.Application/Services/AirQuality/QuartzSchedulerService.cs
using CarbonWise.BuildingBlocks.Application.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Services.AirQuality
{
    public class QuartzSchedulerService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<QuartzSchedulerService> _logger;
        private IScheduler _scheduler;

        public QuartzSchedulerService(IServiceProvider serviceProvider, ILogger<QuartzSchedulerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var factory = new StdSchedulerFactory();
                _scheduler = await factory.GetScheduler(cancellationToken);
                _scheduler.JobFactory = new ServiceProviderJobFactory(_serviceProvider);

                var job = JobBuilder.Create<AirQualityJob>()
                    .WithIdentity("airQualityJob", "airQualityGroup")
                    .Build();

                var trigger = Quartz.TriggerBuilder.Create()
                    .WithIdentity("airQualityTrigger", "airQualityGroup")
                    .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(23, 0)
                        .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time")))
                    .Build();

                await _scheduler.ScheduleJob(job, trigger, cancellationToken);
                await _scheduler.Start(cancellationToken);

                _logger.LogInformation("Quartz Scheduler started successfully. AirQuality job scheduled for 23:00 Turkey time.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start Quartz Scheduler");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_scheduler != null)
                {
                    await _scheduler.Shutdown(cancellationToken);
                    _logger.LogInformation("Quartz Scheduler stopped successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping Quartz Scheduler");
            }
        }
    }

    public class ServiceProviderJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return _serviceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job)
        {
            if (job is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}