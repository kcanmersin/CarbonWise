using System.Linq;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.SchoolInfos;
using Microsoft.EntityFrameworkCore;

namespace CarbonWise.BuildingBlocks.Infrastructure.SchoolInfos
{
    public class SchoolInfoRepository : ISchoolInfoRepository
    {
        private readonly AppDbContext _dbContext;

        public SchoolInfoRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SchoolInfo> GetByIdAsync(SchoolInfoId id)
        {
            return await _dbContext.SchoolInfos
                .Include(s => s.Vehicles)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<SchoolInfo> GetByYearAsync(int year)
        {
            return await _dbContext.SchoolInfos
                .Include(s => s.Vehicles)
                .FirstOrDefaultAsync(s => s.Year == year);
        }

        public async Task AddAsync(SchoolInfo schoolInfo)
        {
            await _dbContext.SchoolInfos.AddAsync(schoolInfo);
        }

        public Task UpdateAsync(SchoolInfo schoolInfo)
        {
            _dbContext.SchoolInfos.Update(schoolInfo);
            return Task.CompletedTask;
        }
    }
}