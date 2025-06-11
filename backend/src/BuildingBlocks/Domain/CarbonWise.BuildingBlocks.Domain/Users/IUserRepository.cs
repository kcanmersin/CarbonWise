using System.Threading.Tasks;
using System.Collections.Generic;

namespace CarbonWise.BuildingBlocks.Domain.Users
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(UserId id);
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByEmailAsync(string email);
        Task<List<User>> GetAllAsync();
        Task<List<User>> GetByRoleAsync(UserRole role);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task<bool> ExistsAsync(string username, string email);
    }
}