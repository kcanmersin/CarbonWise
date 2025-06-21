using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Buildings;

namespace CarbonWise.BuildingBlocks.Domain.Waters
{
    public interface IWaterRepository
    {
        Task<Water> GetByIdAsync(WaterId id);
        Task<List<Water>> GetAllAsync();
        Task<List<Water>> GetByBuildingIdAsync(BuildingId buildingId);
        Task<List<Water>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Water>> GetByBuildingIdAndDateRangeAsync(BuildingId buildingId, DateTime startDate, DateTime endDate);
        Task AddAsync(Water water);
        Task UpdateAsync(Water water);
        Task DeleteAsync(WaterId id);
        Task<List<WaterMonthlyTotalDto>> GetMonthlyTotalsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<WaterMonthlyAggregateDto>> GetMonthlyAggregateAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<bool> ExistsForMonthAsync(BuildingId buildingId, int year, int month);
    }
}