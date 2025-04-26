using CarbonWise.BuildingBlocks.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest
{
    public interface ICarbonFootprintTestRepository
    {
        Task<CarbonFootprintTest> GetByIdAsync(CarbonFootprintTestId id);
        Task<CarbonFootprintTest> GetByIdWithDetailsAsync(CarbonFootprintTestId id);
        Task<List<CarbonFootprintTest>> GetByUserIdAsync(UserId userId);
        Task AddAsync(CarbonFootprintTest test);
        Task UpdateAsync(CarbonFootprintTest test);
        Task UpdateResponseAsync(TestResponseId responseId, TestQuestionOptionId optionId);
    }
}
