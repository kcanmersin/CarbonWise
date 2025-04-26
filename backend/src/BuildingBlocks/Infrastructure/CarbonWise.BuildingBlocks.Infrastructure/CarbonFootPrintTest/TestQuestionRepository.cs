using CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Infrastructure.CarbonFootPrintTest
{
    public class TestQuestionRepository : ITestQuestionRepository
    {
        private readonly AppDbContext _dbContext;

        public TestQuestionRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TestQuestion> GetByIdAsync(TestQuestionId id)
        {
            return await _dbContext.TestQuestions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<List<TestQuestion>> GetAllAsync()
        {
            return await _dbContext.TestQuestions
                .Include(q => q.Options)
                .ToListAsync();
        }

        public async Task<List<TestQuestion>> GetAllOrderedAsync()
        {
            return await _dbContext.TestQuestions
                .Include(q => q.Options)
                .OrderBy(q => q.Category)
                .ThenBy(q => q.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<TestQuestion>> GetByCategoryAsync(string category)
        {
            return await _dbContext.TestQuestions
                .Include(q => q.Options)
                .Where(q => q.Category == category)
                .OrderBy(q => q.DisplayOrder)
                .ToListAsync();
        }
    }
}
