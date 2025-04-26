using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain.Electrics;
using CarbonWise.BuildingBlocks.Domain.NaturalGases;
using CarbonWise.BuildingBlocks.Domain.SchoolInfos;
using CarbonWise.BuildingBlocks.Domain.Users;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using CarbonWise.BuildingBlocks.Domain.Papers;
using CarbonWise.BuildingBlocks.Domain.Waters;
using CarbonWise.BuildingBlocks.Domain.CarbonFootPrintTest;
namespace CarbonWise.BuildingBlocks.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        //school info
        public DbSet<SchoolInfo> SchoolInfos { get; set; }
        public DbSet<CampusVehicleEntry> CampusVehicleEntries { get; set; }

        //building
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Electric> Electrics { get; set; }
        public DbSet<Paper> Papers { get; set; }
        public DbSet<NaturalGas> NaturalGases { get; set; }
        public DbSet<Water> Waters { get; set; }

        public DbSet<CarbonFootprintTest> CarbonFootprintTests { get; set; }
        public DbSet<TestQuestion> TestQuestions { get; set; }
        public DbSet<TestQuestionOption> TestQuestionOptions { get; set; }
        public DbSet<TestResponse> TestResponses { get; set; }
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