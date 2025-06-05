using CarbonWise.BuildingBlocks.Domain.AirQuality;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Infrastructure.AirQuality
{
    public class AirQualityRepository : IAirQualityRepository
    {
        private readonly AppDbContext _dbContext;

        public AirQualityRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Domain.AirQuality.AirQuality> GetByIdAsync(AirQualityId id)
        {
            return await _dbContext.AirQualities
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Domain.AirQuality.AirQuality>> GetByCityAsync(string city)
        {
            return await _dbContext.AirQualities
                .Where(a => a.City == city)
                .OrderByDescending(a => a.RecordDate)
                .ToListAsync();
        }

        public async Task<List<Domain.AirQuality.AirQuality>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.AirQualities
                .Where(a => a.RecordDate >= startDate && a.RecordDate <= endDate)
                .OrderByDescending(a => a.RecordDate)
                .ToListAsync();
        }

        public async Task<List<Domain.AirQuality.AirQuality>> GetByCityAndDateRangeAsync(string city, DateTime startDate, DateTime endDate)
        {
            return await _dbContext.AirQualities
                .Where(a => a.City == city && a.RecordDate >= startDate && a.RecordDate <= endDate)
                .OrderByDescending(a => a.RecordDate)
                .ToListAsync();
        }

        public async Task<List<Domain.AirQuality.AirQuality>> GetLast30DaysAsync(string city = null)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            var query = _dbContext.AirQualities
                .Where(a => a.RecordDate >= cutoffDate);

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(a => a.City == city);
            }

            return await query
                .OrderByDescending(a => a.RecordDate)
                .ToListAsync();
        }

        public async Task<Domain.AirQuality.AirQuality> GetLatestByCityAsync(string city)
        {
            return await _dbContext.AirQualities
                .Where(a => a.City == city)
                .OrderByDescending(a => a.RecordDate)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(Domain.AirQuality.AirQuality airQuality)
        {
            await _dbContext.AirQualities.AddAsync(airQuality);
        }

        public Task UpdateAsync(Domain.AirQuality.AirQuality airQuality)
        {
            _dbContext.AirQualities.Update(airQuality);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(AirQualityId id)
        {
            var airQuality = _dbContext.AirQualities.Find(id);
            if (airQuality != null)
            {
                _dbContext.AirQualities.Remove(airQuality);
            }
            return Task.CompletedTask;
        }

        public async Task DeleteOlderThanAsync(DateTime cutoffDate)
        {
            var oldRecords = await _dbContext.AirQualities
                .Where(a => a.RecordDate < cutoffDate)
                .ToListAsync();

            if (oldRecords.Any())
            {
                _dbContext.AirQualities.RemoveRange(oldRecords);
            }
        }

        public async Task<int> GetCountByCityAsync(string city)
        {
            return await _dbContext.AirQualities
                .CountAsync(a => a.City == city);
        }
    }
}
