using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Domain.Buildings
{
    public interface IBuildingRepository
    {
        Task<Building> GetByIdAsync(BuildingId id);
        Task<List<Building>> GetAllAsync();
        Task<Building> GetByNameAsync(string name);
        Task AddAsync(Building building);
        Task UpdateAsync(Building building);
        Task DeleteAsync(BuildingId id);
    }
}