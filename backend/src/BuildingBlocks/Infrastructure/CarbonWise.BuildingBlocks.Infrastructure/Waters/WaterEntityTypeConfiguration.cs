using CarbonWise.BuildingBlocks.Domain.Waters;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace CarbonWise.BuildingBlocks.Infrastructure.Waters
{
    public class WaterEntityTypeConfiguration : IEntityTypeConfiguration<Water>
    {
        public void Configure(EntityTypeBuilder<Water> builder)
        {
            builder.ToTable("Waters");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasConversion(
                    waterId => waterId.Value,
                    dbId => new WaterId(dbId));

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

            builder.HasIndex(e => e.Date);
        }
    }
}