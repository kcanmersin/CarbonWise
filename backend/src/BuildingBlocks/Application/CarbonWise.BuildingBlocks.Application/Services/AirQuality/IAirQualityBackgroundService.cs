using CarbonWise.BuildingBlocks.Application.Services.ExternalAPIs;
using CarbonWise.BuildingBlocks.Domain.AirQuality;
using CarbonWise.BuildingBlocks.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Services.AirQuality
{
    public interface IAirQualityBackgroundService
    {
        Task FetchAndStoreAirQualityDataAsync();
    }

    public class AirQualityBackgroundService : IAirQualityBackgroundService
    {
        private readonly IExternalAPIsService _externalAPIsService;
        private readonly IAirQualityRepository _airQualityRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AirQualityBackgroundService> _logger;
        private readonly IConfiguration _configuration;

        private readonly string[] _targetCities = { "İstanbul", "Gebze"};

        public AirQualityBackgroundService(
            IExternalAPIsService externalAPIsService,
            IAirQualityRepository airQualityRepository,
            IUnitOfWork unitOfWork,
            ILogger<AirQualityBackgroundService> logger,
            IConfiguration configuration)
        {
            _externalAPIsService = externalAPIsService;
            _airQualityRepository = airQualityRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task FetchAndStoreAirQualityDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting air quality data fetch at {Time}", DateTime.Now);

                foreach (var city in _targetCities)
                {
                    try
                    {
                        await ProcessCityAirQualityAsync(city);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing air quality data for city: {City}", city);
                    }
                }

                await CleanupOldDataAsync();

                _logger.LogInformation("Air quality data fetch completed at {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in air quality background service");
            }
        }

        private async Task ProcessCityAirQualityAsync(string city)
        {
            var result = await _externalAPIsService.GetAirQualityDataAsync(city);

            if (result.Status == "ok" && result.Data != null)
            {
                var turkeyTime = TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));

                var airQuality = Domain.AirQuality.AirQuality.Create(
                    turkeyTime,
                    result.Data.City?.Name ?? city,
                    result.Data.City?.Geo?.Count >= 2 ? result.Data.City.Geo[0] : 0,
                    result.Data.City?.Geo?.Count >= 2 ? result.Data.City.Geo[1] : 0,
                    result.Data.Aqi,
                    result.Data.DominentPol);

                airQuality.UpdateMeasurements(
                    co: result.Data.Iaqi?.Co?.V,
                    humidity: result.Data.Iaqi?.Humidity?.V,
                    no2: result.Data.Iaqi?.No2?.V,
                    ozone: result.Data.Iaqi?.Ozone?.V,
                    pressure: result.Data.Iaqi?.Pressure?.V,
                    pm10: result.Data.Iaqi?.Pm10?.V,
                    pm25: result.Data.Iaqi?.Pm25?.V,
                    so2: result.Data.Iaqi?.So2?.V,
                    temperature: result.Data.Iaqi?.Temperature?.V,
                    windSpeed: result.Data.Iaqi?.WindSpeed?.V);

                await _airQualityRepository.AddAsync(airQuality);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Successfully stored air quality data for {City} with AQI: {AQI}",
                    city, result.Data.Aqi);
            }
            else
            {
                _logger.LogWarning("Failed to fetch air quality data for {City}. Status: {Status}, Error: {Error}",
                    city, result.Status, result.ErrorMessage);
            }
        }

        private async Task CleanupOldDataAsync()
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-30);
                await _airQualityRepository.DeleteOlderThanAsync(cutoffDate);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Cleaned up air quality data older than {CutoffDate}", cutoffDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old air quality data");
            }
        }
    }
}
