using CarbonWise.BuildingBlocks.Domain.Waters;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace CarbonWise.BuildingBlocks.Infrastructure.Waters
{
    public class WaterEntityTypeConfiguration : IEntityTypeConfiguration<Water>
    {
        public void Configure(EntityTypeBuilder<Water> builder)
        {
            builder.ToTable("Waters");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.Id)
                .HasConversion(
                    waterId => waterId.Value,
                    dbId => new WaterId(dbId));

            builder.Property(w => w.Date)
                .IsRequired();

            builder.Property(w => w.InitialMeterValue)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(w => w.FinalMeterValue)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(w => w.Usage)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(w => w.BuildingId)
                .HasConversion(
                    buildingId => buildingId.Value,
                    dbId => new BuildingId(dbId))
                .IsRequired();

            builder.HasOne(w => w.Building)
                .WithMany()
                .HasForeignKey(w => w.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(w => w.Date);
            builder.HasIndex(w => w.BuildingId);
        }
    }
}