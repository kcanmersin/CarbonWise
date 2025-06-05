using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Papers;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using Microsoft.EntityFrameworkCore;

namespace CarbonWise.BuildingBlocks.Infrastructure.Papers
{
    public class PaperRepository : IPaperRepository
    {
        private readonly AppDbContext _dbContext;

        public PaperRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Paper> GetByIdAsync(PaperId id)
        {
            return await _dbContext.Papers
                .Include(p => p.Building)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Paper>> GetAllAsync()
        {
            return await _dbContext.Papers
                .Include(p => p.Building)
                .OrderByDescending(p => p.Date)
                .ToListAsync();
        }

        public async Task<List<Paper>> GetByBuildingIdAsync(BuildingId buildingId)
        {
            return await _dbContext.Papers
                .Include(p => p.Building)
                .Where(p => p.BuildingId == buildingId)
                .OrderByDescending(p => p.Date)
                .ToListAsync();
        }

        public async Task<List<Paper>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.Papers
                .Include(p => p.Building)
                .Where(p => p.Date >= startDate && p.Date <= endDate)
                .OrderByDescending(p => p.Date)
                .ToListAsync();
        }

        public async Task<List<Paper>> GetByBuildingIdAndDateRangeAsync(BuildingId buildingId, DateTime startDate, DateTime endDate)
        {
            return await _dbContext.Papers
                .Include(p => p.Building)
                .Where(p => p.BuildingId == buildingId && p.Date >= startDate && p.Date <= endDate)
                .OrderByDescending(p => p.Date)
                .ToListAsync();
        }

        public async Task AddAsync(Paper paper)
        {
            await _dbContext.Papers.AddAsync(paper);
        }

        public Task UpdateAsync(Paper paper)
        {
            _dbContext.Papers.Update(paper);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(PaperId id)
        {
            var paper = _dbContext.Papers.Find(id);
            if (paper != null)
            {
                _dbContext.Papers.Remove(paper);
            }
            return Task.CompletedTask;
        }

        public async Task<List<PaperMonthlyTotalDto>> GetMonthlyTotalsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbContext.Papers
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(p => p.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.Date <= endDate.Value);

            var paperData = await query
                .Select(p => new
                {
                    p.Date,
                    p.Usage
                })
                .ToListAsync();

            var monthlyTotals = paperData
                .GroupBy(p => new {
                    Year = p.Date.Year,
                    Month = p.Date.Month
                })
                .Select(g => new PaperMonthlyTotalDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalUsage = g.Sum(p => p.Usage)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            return monthlyTotals;
        }

        public async Task<List<PaperMonthlyAggregateDto>> GetMonthlyAggregateAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbContext.Papers
                .Include(p => p.Building)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(p => p.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.Date <= endDate.Value);

            var monthlyData = await query
                .GroupBy(p => new {
                    YearMonth = new DateTime(p.Date.Year, p.Date.Month, 1),
                    BuildingId = p.BuildingId,
                    BuildingName = p.Building.Name
                })
                .Select(g => new PaperMonthlyAggregateDto
                {
                    YearMonth = g.Key.YearMonth,
                    BuildingId = g.Key.BuildingId.Value,
                    BuildingName = g.Key.BuildingName,
                    TotalUsage = g.Sum(p => p.Usage)
                })
                .OrderBy(x => x.YearMonth)
                .ThenBy(x => x.BuildingName)
                .ToListAsync();

            var monthlyTotals = monthlyData
                .GroupBy(x => x.YearMonth)
                .Select(g => new PaperMonthlyAggregateDto
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
    }
}