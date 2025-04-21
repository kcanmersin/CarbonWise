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
    }
}