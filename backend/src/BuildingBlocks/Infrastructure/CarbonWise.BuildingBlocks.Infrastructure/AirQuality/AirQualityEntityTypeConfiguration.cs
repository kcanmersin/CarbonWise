using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.AirQuality;

namespace CarbonWise.BuildingBlocks.Infrastructure.AirQuality
{
    public class AirQualityEntityTypeConfiguration : IEntityTypeConfiguration<Domain.AirQuality.AirQuality>
    {
        public void Configure(EntityTypeBuilder<Domain.AirQuality.AirQuality> builder)
        {
            builder.ToTable("AirQualities");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .HasConversion(
                    airQualityId => airQualityId.Value,
                    dbId => new AirQualityId(dbId));

            builder.Property(a => a.RecordDate)
                .IsRequired();

            builder.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.Latitude)
                .IsRequired()
                .HasPrecision(10, 7);

            builder.Property(a => a.Longitude)
                .IsRequired()
                .HasPrecision(10, 7);

            builder.Property(a => a.AQI)
                .IsRequired();

            builder.Property(a => a.DominentPollutant)
                .HasMaxLength(50);

            builder.Property(a => a.CO)
                .HasPrecision(10, 2);

            builder.Property(a => a.Humidity)
                .HasPrecision(10, 2);

            builder.Property(a => a.NO2)
                .HasPrecision(10, 2);

            builder.Property(a => a.Ozone)
                .HasPrecision(10, 2);

            builder.Property(a => a.Pressure)
                .HasPrecision(10, 2);

            builder.Property(a => a.PM10)
                .HasPrecision(10, 2);

            builder.Property(a => a.PM25)
                .HasPrecision(10, 2);

            builder.Property(a => a.SO2)
                .HasPrecision(10, 2);

            builder.Property(a => a.Temperature)
                .HasPrecision(10, 2);

            builder.Property(a => a.WindSpeed)
                .HasPrecision(10, 2);

            builder.Property(a => a.CreatedAt)
                .IsRequired();

            builder.HasIndex(a => a.City);
            builder.HasIndex(a => a.RecordDate);
            builder.HasIndex(a => new { a.City, a.RecordDate });
        }
    }
}
