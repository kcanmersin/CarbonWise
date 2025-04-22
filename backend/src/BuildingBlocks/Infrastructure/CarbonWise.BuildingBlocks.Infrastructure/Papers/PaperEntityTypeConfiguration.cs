using CarbonWise.BuildingBlocks.Domain.Papers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarbonWise.BuildingBlocks.Infrastructure.Papers
{
    public class PaperEntityTypeConfiguration : IEntityTypeConfiguration<Paper>
    {
        public void Configure(EntityTypeBuilder<Paper> builder)
        {
            builder.ToTable("Papers");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasConversion(
                    paperId => paperId.Value,
                    dbId => new PaperId(dbId));

            builder.Property(e => e.Date)
                .IsRequired();

            builder.Property(e => e.Usage)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.HasIndex(e => e.Date);
        }
    }
}