// src/BuildingBlocks/Infrastructure/CarbonWise.BuildingBlocks.Infrastructure/NaturalGas/NaturalGasEntityTypeConfiguration.cs
using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain.NaturalGases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarbonWise.BuildingBlocks.Infrastructure.NaturalGases
{
    public class NaturalGasEntityTypeConfiguration : IEntityTypeConfiguration<NaturalGas>
    {
        public void Configure(EntityTypeBuilder<NaturalGas> builder)
        {
            builder.ToTable("NaturalGas");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasConversion(
                    naturalGasId => naturalGasId.Value,
                    dbId => new NaturalGasId(dbId));

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

            builder.Property(e => e.SM3Value)
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

