using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using Microsoft.EntityFrameworkCore;

namespace CarbonWise.BuildingBlocks.Infrastructure.Buildings
{
    public class BuildingRepository : IBuildingRepository
    {
        private readonly AppDbContext _dbContext;

        public BuildingRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Building> GetByIdAsync(BuildingId id)
        {
            return await _dbContext.Buildings.FindAsync(id);
        }

        public async Task<List<Building>> GetAllAsync()
        {
            return await _dbContext.Buildings.ToListAsync();
        }

        public async Task<Building> GetByNameAsync(string name)
        {
            return await _dbContext.Buildings
                .Where(b => b.Name == name)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(Building building)
        {
            await _dbContext.Buildings.AddAsync(building);
        }

        public Task UpdateAsync(Building building)
        {
            _dbContext.Buildings.Update(building);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(BuildingId id)
        {
            var building = _dbContext.Buildings.Find(id);
            if (building != null)
            {
                _dbContext.Buildings.Remove(building);
            }
            return Task.CompletedTask;
        }
    }
}