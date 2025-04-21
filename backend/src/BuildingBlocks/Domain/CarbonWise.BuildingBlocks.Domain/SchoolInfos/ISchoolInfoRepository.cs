using CarbonWise.BuildingBlocks.Domain.SchoolInfos;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Domain.SchoolInfos
{
    public interface ISchoolInfoRepository
    {
        Task<SchoolInfo> GetByIdAsync(SchoolInfoId id);
        Task<SchoolInfo> GetByYearAsync(int year);
        Task AddAsync(SchoolInfo schoolInfo);
        Task UpdateAsync(SchoolInfo schoolInfo);
    }
}