using CarbonWise.BuildingBlocks.Application.Services.AirQuality;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Jobs
{
    [DisallowConcurrentExecution]
    public class AirQualityJob : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AirQualityJob> _logger;

        public AirQualityJob(IServiceProvider serviceProvider, ILogger<AirQualityJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("AirQualityJob started at {Time}", DateTime.Now);

                using var scope = _serviceProvider.CreateScope();
                var airQualityService = scope.ServiceProvider.GetRequiredService<IAirQualityBackgroundService>();

                await airQualityService.FetchAndStoreAirQualityDataAsync();

                _logger.LogInformation("AirQualityJob completed successfully at {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AirQualityJob failed at {Time}", DateTime.Now);
                throw;
            }
        }
    }
}
