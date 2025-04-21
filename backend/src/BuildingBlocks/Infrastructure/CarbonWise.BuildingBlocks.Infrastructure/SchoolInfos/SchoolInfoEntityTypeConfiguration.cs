using CarbonWise.BuildingBlocks.Domain.SchoolInfos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarbonWise.BuildingBlocks.Infrastructure.SchoolInfos
{
    public class SchoolInfoEntityTypeConfiguration : IEntityTypeConfiguration<SchoolInfo>
    {
        public void Configure(EntityTypeBuilder<SchoolInfo> builder)
        {
            builder.ToTable("SchoolInfos");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .HasConversion(
                    schoolInfoId => schoolInfoId.Value,
                    dbId => new SchoolInfoId(dbId));

            builder.Property(s => s.NumberOfPeople)
                .IsRequired();

            builder.Property(s => s.Year)
                .IsRequired();

            builder.Property(s => s.CampusVehicleEntryId)
                .HasConversion(
                    vehicleEntryId => vehicleEntryId != null ? vehicleEntryId.Value : (Guid?)null,
                    dbId => dbId.HasValue ? new CampusVehicleEntryId(dbId.Value) : null);

            builder.HasOne(s => s.Vehicles)
                .WithMany()
                .HasForeignKey(s => s.CampusVehicleEntryId);

            builder.HasIndex(s => s.Year)
                .IsUnique();
        }
    }
}