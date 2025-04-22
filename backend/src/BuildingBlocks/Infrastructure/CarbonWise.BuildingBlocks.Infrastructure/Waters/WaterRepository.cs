using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Waters;
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
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Water>> GetAllAsync()
        {
            return await _dbContext.Waters
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<List<Water>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.Waters
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .OrderByDescending(e => e.Date)
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
    }
}