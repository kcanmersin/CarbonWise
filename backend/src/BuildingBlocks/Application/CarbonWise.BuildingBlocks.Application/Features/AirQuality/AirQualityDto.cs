using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Features.AirQuality
{
    public class AirQualityDto
    {
        public Guid Id { get; set; }
        public DateTime RecordDate { get; set; }
        public string City { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int AQI { get; set; }
        public string DominentPollutant { get; set; }
        public double? CO { get; set; }
        public double? Humidity { get; set; }
        public double? NO2 { get; set; }
        public double? Ozone { get; set; }
        public double? Pressure { get; set; }
        public double? PM10 { get; set; }
        public double? PM25 { get; set; }
        public double? SO2 { get; set; }
        public double? Temperature { get; set; }
        public double? WindSpeed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
