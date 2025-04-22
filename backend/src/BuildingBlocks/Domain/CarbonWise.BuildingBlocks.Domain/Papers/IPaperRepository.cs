using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Domain.Papers
{
    public interface IPaperRepository
    {
        Task<Paper> GetByIdAsync(PaperId id);
        Task<List<Paper>> GetAllAsync();
        Task<List<Paper>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task AddAsync(Paper paper);
        Task UpdateAsync(Paper paper);
        Task DeleteAsync(PaperId id);
    }
}