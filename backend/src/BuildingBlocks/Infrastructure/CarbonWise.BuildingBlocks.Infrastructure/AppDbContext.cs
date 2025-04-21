using CarbonWise.BuildingBlocks.Domain.SchoolInfos;
using CarbonWise.BuildingBlocks.Domain.Users;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CarbonWise.BuildingBlocks.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        //school info
        public DbSet<SchoolInfo> SchoolInfos { get; set; }
        public DbSet<CampusVehicleEntry> CampusVehicleEntries { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            await base.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}