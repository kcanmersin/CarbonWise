using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Domain.Waters
{
    public interface IWaterRepository
    {
        Task<Water> GetByIdAsync(WaterId id);
        Task<List<Water>> GetAllAsync();
        Task<List<Water>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task AddAsync(Water water);
        Task UpdateAsync(Water water);
        Task DeleteAsync(WaterId id);
    }
}