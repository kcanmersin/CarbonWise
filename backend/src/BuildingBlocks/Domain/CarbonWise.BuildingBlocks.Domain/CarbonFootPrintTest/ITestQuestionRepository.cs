using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest
{
    public interface ITestQuestionRepository
    {
        Task<TestQuestion> GetByIdAsync(TestQuestionId id);
        Task<List<TestQuestion>> GetAllAsync();
        Task<List<TestQuestion>> GetAllOrderedAsync();
        Task<List<TestQuestion>> GetByCategoryAsync(string category);
    }
}
