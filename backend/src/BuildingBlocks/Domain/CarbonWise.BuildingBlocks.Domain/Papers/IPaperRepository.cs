using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain.Waters;

namespace CarbonWise.BuildingBlocks.Domain.Papers
{
    public interface IPaperRepository
    {
        Task<Paper> GetByIdAsync(PaperId id);
        Task<List<Paper>> GetAllAsync();
        Task<List<Paper>> GetByBuildingIdAsync(BuildingId buildingId);
        Task<List<Paper>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Paper>> GetByBuildingIdAndDateRangeAsync(BuildingId buildingId, DateTime startDate, DateTime endDate);
        Task AddAsync(Paper paper);
        Task UpdateAsync(Paper paper);
        Task DeleteAsync(PaperId id);
        Task<List<PaperMonthlyTotalDto>> GetMonthlyTotalsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<PaperMonthlyAggregateDto>> GetMonthlyAggregateAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}