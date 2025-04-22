using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Papers;
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
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Paper>> GetAllAsync()
        {
            return await _dbContext.Papers
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<List<Paper>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.Papers
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .OrderByDescending(e => e.Date)
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
    }
}