using CarbonWise.BuildingBlocks.Domain.SchoolInfos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarbonWise.BuildingBlocks.Infrastructure.SchoolInfos
{
    public class CampusVehicleEntryEntityTypeConfiguration : IEntityTypeConfiguration<CampusVehicleEntry>
    {
        public void Configure(EntityTypeBuilder<CampusVehicleEntry> builder)
        {
            builder.ToTable("CampusVehicleEntries");

            builder.HasKey(v => v.Id);

            builder.Property(v => v.Id)
                .HasConversion(
                    vehicleEntryId => vehicleEntryId.Value,
                    dbId => new CampusVehicleEntryId(dbId));

            builder.Property(v => v.CarsManagedByUniversity)
                .IsRequired();

            builder.Property(v => v.CarsEnteringUniversity)
                .IsRequired();

            builder.Property(v => v.MotorcyclesEnteringUniversity)
                .IsRequired();
        }
    }
}