using System.Linq;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CarbonWise.BuildingBlocks.Infrastructure.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> GetByIdAsync(UserId id)
        {
            return await _dbContext.Users.FindAsync(id);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _dbContext.Users
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbContext.Users
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
        }

        public Task UpdateAsync(User user)
        {
            _dbContext.Users.Update(user);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(string username, string email)
        {
            return await _dbContext.Users
                .AnyAsync(u => u.Username == username || u.Email == email);
        }
    }
}