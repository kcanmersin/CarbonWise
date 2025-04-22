using CarbonWise.BuildingBlocks.Domain.Buildings;

namespace CarbonWise.BuildingBlocks.Domain.NaturalGases
{
    public interface INaturalGasRepository
    {
        Task<NaturalGas> GetByIdAsync(NaturalGasId id);
        Task<List<NaturalGas>> GetByBuildingIdAsync(BuildingId buildingId);
        Task<List<NaturalGas>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<NaturalGas>> GetByBuildingIdAndDateRangeAsync(BuildingId buildingId, DateTime startDate, DateTime endDate);
        Task AddAsync(NaturalGas naturalGas);
        Task UpdateAsync(NaturalGas naturalGas);
        Task DeleteAsync(NaturalGasId id);
    }
}