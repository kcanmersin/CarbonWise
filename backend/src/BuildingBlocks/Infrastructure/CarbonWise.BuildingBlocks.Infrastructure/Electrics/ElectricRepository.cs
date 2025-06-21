using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain.Electrics;
using Microsoft.EntityFrameworkCore;

namespace CarbonWise.BuildingBlocks.Infrastructure.Electrics
{
    public class ElectricRepository : IElectricRepository
    {
        private readonly AppDbContext _dbContext;

        public ElectricRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Electric> GetByIdAsync(ElectricId id)
        {
            return await _dbContext.Electrics
                .Include(e => e.Building)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Electric>> GetByBuildingIdAsync(BuildingId buildingId)
        {
            return await _dbContext.Electrics
                .Include(e => e.Building)
                .Where(e => e.BuildingId == buildingId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<List<Electric>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.Electrics
                .Include(e => e.Building)
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<List<Electric>> GetByBuildingIdAndDateRangeAsync(BuildingId buildingId, DateTime startDate, DateTime endDate)
        {
            return await _dbContext.Electrics
                .Include(e => e.Building)
                .Where(e => e.BuildingId == buildingId && e.Date >= startDate && e.Date <= endDate)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task AddAsync(Electric electric)
        {
            await _dbContext.Electrics.AddAsync(electric);
        }

        public Task UpdateAsync(Electric electric)
        {
            _dbContext.Electrics.Update(electric);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ElectricId id)
        {
            var electric = _dbContext.Electrics.Find(id);
            if (electric != null)
            {
                _dbContext.Electrics.Remove(electric);
            }
            return Task.CompletedTask;
        }
        public async Task<List<ElectricMonthlyTotalDto>> GetMonthlyTotalsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbContext.Electrics
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(e => e.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.Date <= endDate.Value);

            var electricData = await query
                .Select(e => new
                {
                    e.Date,
                    e.KWHValue,
                    e.Usage
                })
                .ToListAsync();

            var monthlyTotals = electricData
                .GroupBy(e => new {
                    Year = e.Date.Year,
                    Month = e.Date.Month
                })
                .Select(g => new ElectricMonthlyTotalDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalKWHValue = g.Sum(e => e.KWHValue),
                    TotalUsage = g.Sum(e => e.Usage)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            return monthlyTotals;
        }
        public async Task<List<ElectricMonthlyAggregateDto>> GetMonthlyAggregateAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbContext.Electrics
                .Include(e => e.Building)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(e => e.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.Date <= endDate.Value);

            var electricData = await query
                .Select(e => new
                {
                    e.Date,
                    e.KWHValue,
                    e.Usage,
                    BuildingId = e.BuildingId.Value,
                    BuildingName = e.Building.Name
                })
                .ToListAsync();

            var monthlyData = electricData
                .GroupBy(e => new {
                    YearMonth = new DateTime(e.Date.Year, e.Date.Month, 1),
                    BuildingId = e.BuildingId,
                    BuildingName = e.BuildingName
                })
                .Select(g => new ElectricMonthlyAggregateDto
                {
                    YearMonth = g.Key.YearMonth,
                    BuildingId = g.Key.BuildingId,
                    BuildingName = g.Key.BuildingName,
                    TotalKWHValue = g.Sum(e => e.KWHValue),
                    TotalUsage = g.Sum(e => e.Usage)
                })
                .ToList();

            var monthlyTotals = monthlyData
                .GroupBy(x => x.YearMonth)
                .Select(g => new ElectricMonthlyAggregateDto
                {
                    YearMonth = g.Key,
                    BuildingId = Guid.Empty,
                    BuildingName = "Total",
                    TotalKWHValue = g.Sum(x => x.TotalKWHValue),
                    TotalUsage = g.Sum(x => x.TotalUsage)
                })
                .ToList();

            monthlyData.AddRange(monthlyTotals);

            return monthlyData
                .OrderBy(x => x.YearMonth)
                .ThenBy(x => x.BuildingName == "Total" ? 1 : 0)
                .ToList();
        }

        public async Task<bool> ExistsForMonthAsync(BuildingId buildingId, int year, int month)
        {
            return await _dbContext.Electrics
                .AnyAsync(e => e.BuildingId == buildingId && 
                              e.Date.Year == year && 
                              e.Date.Month == month);
        }
    }
}