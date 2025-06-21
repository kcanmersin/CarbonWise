using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Buildings;

namespace CarbonWise.BuildingBlocks.Domain.Electrics
{
    public interface IElectricRepository
    {
        Task<Electric> GetByIdAsync(ElectricId id);
        Task<List<Electric>> GetByBuildingIdAsync(BuildingId buildingId);
        Task<List<Electric>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Electric>> GetByBuildingIdAndDateRangeAsync(BuildingId buildingId, DateTime startDate, DateTime endDate);
        Task AddAsync(Electric electric);
        Task UpdateAsync(Electric electric);
        Task DeleteAsync(ElectricId id);
        Task<List<ElectricMonthlyTotalDto>> GetMonthlyTotalsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<ElectricMonthlyAggregateDto>> GetMonthlyAggregateAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<bool> ExistsForMonthAsync(BuildingId buildingId, int year, int month);
    }
}