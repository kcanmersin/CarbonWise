using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Domain.AirQuality
{
    public interface IAirQualityRepository
    {
        Task<AirQuality> GetByIdAsync(AirQualityId id);
        Task<List<AirQuality>> GetByCityAsync(string city);
        Task<List<AirQuality>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<AirQuality>> GetByCityAndDateRangeAsync(string city, DateTime startDate, DateTime endDate);
        Task<List<AirQuality>> GetLast30DaysAsync(string city = null);
        Task<AirQuality> GetLatestByCityAsync(string city);
        Task AddAsync(AirQuality airQuality);
        Task UpdateAsync(AirQuality airQuality);
        Task DeleteAsync(AirQualityId id);
        Task DeleteOlderThanAsync(DateTime cutoffDate);
        Task<int> GetCountByCityAsync(string city);
    }
}
