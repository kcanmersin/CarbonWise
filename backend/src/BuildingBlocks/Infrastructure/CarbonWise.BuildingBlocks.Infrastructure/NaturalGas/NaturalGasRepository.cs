using CarbonWise.BuildingBlocks.Application.Features.NaturalGases;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain.NaturalGases;
using Microsoft.EntityFrameworkCore;

namespace CarbonWise.BuildingBlocks.Infrastructure.NaturalGases
{
    public class NaturalGasRepository : INaturalGasRepository
    {
        private readonly AppDbContext _dbContext;

        public NaturalGasRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<NaturalGas> GetByIdAsync(NaturalGasId id)
        {
            return await _dbContext.NaturalGases
                .Include(e => e.Building)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<NaturalGas>> GetByBuildingIdAsync(BuildingId buildingId)
        {
            return await _dbContext.NaturalGases
                .Include(e => e.Building)
                .Where(e => e.BuildingId == buildingId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<List<NaturalGas>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.NaturalGases
                .Include(e => e.Building)
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<List<NaturalGas>> GetByBuildingIdAndDateRangeAsync(BuildingId buildingId, DateTime startDate, DateTime endDate)
        {
            return await _dbContext.NaturalGases
                .Include(e => e.Building)
                .Where(e => e.BuildingId == buildingId && e.Date >= startDate && e.Date <= endDate)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task AddAsync(NaturalGas naturalGas)
        {
            await _dbContext.NaturalGases.AddAsync(naturalGas);
        }

        public Task UpdateAsync(NaturalGas naturalGas)
        {
            _dbContext.NaturalGases.Update(naturalGas);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(NaturalGasId id)
        {
            var naturalGas = _dbContext.NaturalGases.Find(id);
            if (naturalGas != null)
            {
                _dbContext.NaturalGases.Remove(naturalGas);
            }
            return Task.CompletedTask;
        }
        public async Task<List<NaturalGasMonthlyTotalDto>> GetMonthlyTotalsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbContext.NaturalGases
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(e => e.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.Date <= endDate.Value);

            var naturalGasData = await query
                .Select(e => new
                {
                    e.Date,
                    e.SM3Value,
                    e.Usage
                })
                .ToListAsync();

            var monthlyTotals = naturalGasData
                .GroupBy(e => new {
                    Year = e.Date.Year,
                    Month = e.Date.Month
                })
                .Select(g => new NaturalGasMonthlyTotalDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalSM3Value = g.Sum(e => e.SM3Value),
                    TotalUsage = g.Sum(e => e.Usage)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            return monthlyTotals;
        }
        public async Task<List<NaturalGasMonthlyAggregateDto>> GetMonthlyAggregateAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbContext.NaturalGases
                .Include(e => e.Building)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(e => e.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.Date <= endDate.Value);

            var monthlyData = await query
                .GroupBy(e => new {
                    YearMonth = new DateTime(e.Date.Year, e.Date.Month, 1),
                    BuildingId = e.BuildingId,
                    BuildingName = e.Building.Name
                })
                .Select(g => new NaturalGasMonthlyAggregateDto
                {
                    YearMonth = g.Key.YearMonth,
                    BuildingId = g.Key.BuildingId.Value,
                    BuildingName = g.Key.BuildingName,
                    TotalSM3Value = g.Sum(e => e.SM3Value),
                    TotalUsage = g.Sum(e => e.Usage)
                })
                .OrderBy(x => x.YearMonth)
                .ThenBy(x => x.BuildingName)
                .ToListAsync();

            var monthlyTotals = monthlyData
                .GroupBy(x => x.YearMonth)
                .Select(g => new NaturalGasMonthlyAggregateDto
                {
                    YearMonth = g.Key,
                    BuildingId = Guid.Empty,
                    BuildingName = "Total",
                    TotalSM3Value = g.Sum(x => x.TotalSM3Value),
                    TotalUsage = g.Sum(x => x.TotalUsage)
                })
                .ToList();

            monthlyData.AddRange(monthlyTotals);

            return monthlyData.OrderBy(x => x.YearMonth).ThenBy(x => x.BuildingName == "Total" ? 1 : 0).ToList();
        }
    }
}

