using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Waters;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using Microsoft.EntityFrameworkCore;

namespace CarbonWise.BuildingBlocks.Infrastructure.Waters
{
    public class WaterRepository : IWaterRepository
    {
        private readonly AppDbContext _dbContext;

        public WaterRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Water> GetByIdAsync(WaterId id)
        {
            return await _dbContext.Waters
                .Include(w => w.Building)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<List<Water>> GetAllAsync()
        {
            return await _dbContext.Waters
                .Include(w => w.Building)
                .OrderByDescending(w => w.Date)
                .ToListAsync();
        }

        public async Task<List<Water>> GetByBuildingIdAsync(BuildingId buildingId)
        {
            return await _dbContext.Waters
                .Include(w => w.Building)
                .Where(w => w.BuildingId == buildingId)
                .OrderByDescending(w => w.Date)
                .ToListAsync();
        }

        public async Task<List<Water>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.Waters
                .Include(w => w.Building)
                .Where(w => w.Date >= startDate && w.Date <= endDate)
                .OrderByDescending(w => w.Date)
                .ToListAsync();
        }

        public async Task<List<Water>> GetByBuildingIdAndDateRangeAsync(BuildingId buildingId, DateTime startDate, DateTime endDate)
        {
            return await _dbContext.Waters
                .Include(w => w.Building)
                .Where(w => w.BuildingId == buildingId && w.Date >= startDate && w.Date <= endDate)
                .OrderByDescending(w => w.Date)
                .ToListAsync();
        }

        public async Task AddAsync(Water water)
        {
            await _dbContext.Waters.AddAsync(water);
        }

        public Task UpdateAsync(Water water)
        {
            _dbContext.Waters.Update(water);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(WaterId id)
        {
            var water = _dbContext.Waters.Find(id);
            if (water != null)
            {
                _dbContext.Waters.Remove(water);
            }
            return Task.CompletedTask;
        }

        public async Task<List<WaterMonthlyTotalDto>> GetMonthlyTotalsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbContext.Waters
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(w => w.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(w => w.Date <= endDate.Value);

            var waterData = await query
                .Select(w => new
                {
                    w.Date,
                    w.Usage
                })
                .ToListAsync();

            var monthlyTotals = waterData
                .GroupBy(w => new {
                    Year = w.Date.Year,
                    Month = w.Date.Month
                })
                .Select(g => new WaterMonthlyTotalDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalUsage = g.Sum(w => w.Usage)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            return monthlyTotals;
        }

        public async Task<List<WaterMonthlyAggregateDto>> GetMonthlyAggregateAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbContext.Waters
                .Include(w => w.Building)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(w => w.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(w => w.Date <= endDate.Value);

            var monthlyData = await query
                .GroupBy(w => new {
                    YearMonth = new DateTime(w.Date.Year, w.Date.Month, 1),
                    BuildingId = w.BuildingId,
                    BuildingName = w.Building.Name
                })
                .Select(g => new WaterMonthlyAggregateDto
                {
                    YearMonth = g.Key.YearMonth,
                    BuildingId = g.Key.BuildingId.Value,
                    BuildingName = g.Key.BuildingName,
                    TotalUsage = g.Sum(w => w.Usage)
                })
                .OrderBy(x => x.YearMonth)
                .ThenBy(x => x.BuildingName)
                .ToListAsync();

            var monthlyTotals = monthlyData
                .GroupBy(x => x.YearMonth)
                .Select(g => new WaterMonthlyAggregateDto
                {
                    YearMonth = g.Key,
                    BuildingId = Guid.Empty,
                    BuildingName = "Total",
                    TotalUsage = g.Sum(x => x.TotalUsage)
                })
                .ToList();

            monthlyData.AddRange(monthlyTotals);

            return monthlyData.OrderBy(x => x.YearMonth).ThenBy(x => x.BuildingName == "Total" ? 1 : 0).ToList();
        }

        public async Task<bool> ExistsForMonthAsync(BuildingId buildingId, int year, int month)
        {
            return await _dbContext.Waters
                .AnyAsync(w => w.BuildingId == buildingId && 
                              w.Date.Year == year && 
                              w.Date.Month == month);
        }
    }
}