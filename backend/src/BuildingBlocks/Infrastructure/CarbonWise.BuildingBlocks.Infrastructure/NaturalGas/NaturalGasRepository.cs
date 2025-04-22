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
    }
}

