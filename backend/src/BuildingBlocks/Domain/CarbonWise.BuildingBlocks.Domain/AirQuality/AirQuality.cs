using System;
using CarbonWise.BuildingBlocks.Domain;

namespace CarbonWise.BuildingBlocks.Domain.AirQuality
{
    public class AirQuality : Entity, IAggregateRoot
    {
        public AirQualityId Id { get; private set; }
        public DateTime RecordDate { get; private set; }
        public string City { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public int AQI { get; private set; }
        public string DominentPollutant { get; private set; }

        public double? CO { get; private set; }
        public double? Humidity { get; private set; }
        public double? NO2 { get; private set; }
        public double? Ozone { get; private set; }
        public double? Pressure { get; private set; }
        public double? PM10 { get; private set; }
        public double? PM25 { get; private set; }
        public double? SO2 { get; private set; }
        public double? Temperature { get; private set; }
        public double? WindSpeed { get; private set; }

        public DateTime CreatedAt { get; private set; }

        protected AirQuality() { }

        private AirQuality(
            AirQualityId id,
            DateTime recordDate,
            string city,
            double latitude,
            double longitude,
            int aqi,
            string dominentPollutant)
        {
            Id = id;
            RecordDate = recordDate;
            City = city;
            Latitude = latitude;
            Longitude = longitude;
            AQI = aqi;
            DominentPollutant = dominentPollutant;
            CreatedAt = DateTime.UtcNow;
        }

        public static AirQuality Create(
            DateTime recordDate,
            string city,
            double latitude,
            double longitude,
            int aqi,
            string dominentPollutant = null)
        {
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City cannot be empty", nameof(city));

            if (aqi < 0)
                throw new ArgumentException("AQI cannot be negative", nameof(aqi));

            var airQuality = new AirQuality(
                new AirQualityId(Guid.NewGuid()),
                recordDate,
                city,
                latitude,
                longitude,
                aqi,
                dominentPollutant);

            airQuality.AddDomainEvent(new AirQualityCreatedDomainEvent(airQuality.Id));

            return airQuality;
        }

        public void UpdateMeasurements(
            double? co = null,
            double? humidity = null,
            double? no2 = null,
            double? ozone = null,
            double? pressure = null,
            double? pm10 = null,
            double? pm25 = null,
            double? so2 = null,
            double? temperature = null,
            double? windSpeed = null)
        {
            CO = co;
            Humidity = humidity;
            NO2 = no2;
            Ozone = ozone;
            Pressure = pressure;
            PM10 = pm10;
            PM25 = pm25;
            SO2 = so2;
            Temperature = temperature;
            WindSpeed = windSpeed;
        }
    }
}


