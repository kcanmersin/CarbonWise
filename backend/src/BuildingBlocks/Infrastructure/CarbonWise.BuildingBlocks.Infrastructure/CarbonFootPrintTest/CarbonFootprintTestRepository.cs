using CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest;
using CarbonWise.BuildingBlocks.Domain.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Infrastructure.CarbonFootPrintTest
{
    public class CarbonFootprintTestRepository : ICarbonFootprintTestRepository
    {
        private readonly AppDbContext _dbContext;

        public CarbonFootprintTestRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CarbonFootprintTest> GetByIdAsync(CarbonFootprintTestId id)
        {
            return await _dbContext.CarbonFootprintTests
                .Include(t => t.Responses)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<CarbonFootprintTest> GetByIdWithDetailsAsync(CarbonFootprintTestId id)
        {
            return await _dbContext.CarbonFootprintTests
                .Include(t => t.Responses)
                    .ThenInclude(r => r.Question)
                .Include(t => t.Responses)
                    .ThenInclude(r => r.SelectedOption)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<CarbonFootprintTest>> GetByUserIdAsync(UserId userId)
        {
            return await _dbContext.CarbonFootprintTests
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CompletedAt)
                .ToListAsync();
        }

        public async Task AddAsync(CarbonFootprintTest test)
        {
            await _dbContext.CarbonFootprintTests.AddAsync(test);
        }

        public Task UpdateAsync(CarbonFootprintTest test)
        {
            _dbContext.CarbonFootprintTests.Update(test);
            return Task.CompletedTask;
        }

        public async Task UpdateResponseAsync(TestResponseId responseId, TestQuestionOptionId optionId)
        {
            var response = await _dbContext.TestResponses
                .FirstOrDefaultAsync(r => r.Id == responseId);

            if (response != null)
            {
                _dbContext.TestResponses.Remove(response);

                var newResponse = TestResponse.Create(
                    response.TestId,
                    response.QuestionId,
                    optionId);

                await _dbContext.TestResponses.AddAsync(newResponse);
            }
        }
    }
}
