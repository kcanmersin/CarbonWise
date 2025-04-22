using CarbonWise.BuildingBlocks.Domain.Buildings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarbonWise.BuildingBlocks.Infrastructure.Buildings
{
    public class BuildingEntityTypeConfiguration : IEntityTypeConfiguration<Building>
    {
        public void Configure(EntityTypeBuilder<Building> builder)
        {
            builder.ToTable("Buildings");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id)
                .HasConversion(
                    buildingId => buildingId.Value,
                    dbId => new BuildingId(dbId));

            builder.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(100);


            builder.Property(b => b.E_MeterCode)
                .IsRequired(false)
                .HasMaxLength(20);

            builder.Property(b => b.G_MeterCode)
                .IsRequired(false)
                .HasMaxLength(20);


            builder.HasIndex(b => b.Name)
                .IsUnique();
        }
    }
}