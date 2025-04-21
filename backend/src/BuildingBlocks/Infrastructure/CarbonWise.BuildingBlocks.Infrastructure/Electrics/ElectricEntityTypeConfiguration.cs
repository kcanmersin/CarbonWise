using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain.Electrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarbonWise.BuildingBlocks.Infrastructure.Electrics
{
    public class ElectricEntityTypeConfiguration : IEntityTypeConfiguration<Electric>
    {
        public void Configure(EntityTypeBuilder<Electric> builder)
        {
            builder.ToTable("Electrics");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasConversion(
                    electricId => electricId.Value,
                    dbId => new ElectricId(dbId));

            builder.Property(e => e.Date)
                .IsRequired();

            builder.Property(e => e.InitialMeterValue)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(e => e.FinalMeterValue)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(e => e.Usage)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(e => e.KWHValue)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(e => e.BuildingId)
                .HasConversion(
                    buildingId => buildingId.Value,
                    dbId => new BuildingId(dbId))
                .IsRequired();

            builder.HasOne(e => e.Building)
                .WithMany()
                .HasForeignKey(e => e.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => e.Date);
            builder.HasIndex(e => e.BuildingId);
        }
    }
}