using CarbonWise.BuildingBlocks.Domain.Papers;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarbonWise.BuildingBlocks.Infrastructure.Papers
{
    public class PaperEntityTypeConfiguration : IEntityTypeConfiguration<Paper>
    {
        public void Configure(EntityTypeBuilder<Paper> builder)
        {
            builder.ToTable("Papers");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasConversion(
                    paperId => paperId.Value,
                    dbId => new PaperId(dbId));

            builder.Property(p => p.Date)
                .IsRequired();

            builder.Property(p => p.Usage)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(p => p.BuildingId)
                .HasConversion(
                    buildingId => buildingId.Value,
                    dbId => new BuildingId(dbId))
                .IsRequired();

            builder.HasOne(p => p.Building)
                .WithMany()
                .HasForeignKey(p => p.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.Date);
            builder.HasIndex(p => p.BuildingId);
        }
    }
}