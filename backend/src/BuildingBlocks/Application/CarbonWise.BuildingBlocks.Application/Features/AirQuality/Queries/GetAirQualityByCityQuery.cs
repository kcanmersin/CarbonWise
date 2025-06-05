using CarbonWise.BuildingBlocks.Domain.AirQuality;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Features.AirQuality.Queries
{
    public class GetAirQualityByCityQuery : IRequest<List<AirQualityDto>>
    {
        public string City { get; set; }
    }

    public class GetAirQualityByCityQueryHandler : IRequestHandler<GetAirQualityByCityQuery, List<AirQualityDto>>
    {
        private readonly IAirQualityRepository _airQualityRepository;

        public GetAirQualityByCityQueryHandler(IAirQualityRepository airQualityRepository)
        {
            _airQualityRepository = airQualityRepository;
        }

        public async Task<List<AirQualityDto>> Handle(GetAirQualityByCityQuery request, CancellationToken cancellationToken)
        {
            var airQualities = await _airQualityRepository.GetByCityAsync(request.City);

            return airQualities.Select(a => new AirQualityDto
            {
                Id = a.Id.Value,
                RecordDate = a.RecordDate,
                City = a.City,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                AQI = a.AQI,
                DominentPollutant = a.DominentPollutant,
                CO = a.CO,
                Humidity = a.Humidity,
                NO2 = a.NO2,
                Ozone = a.Ozone,
                Pressure = a.Pressure,
                PM10 = a.PM10,
                PM25 = a.PM25,
                SO2 = a.SO2,
                Temperature = a.Temperature,
                WindSpeed = a.WindSpeed,
                CreatedAt = a.CreatedAt
            }).ToList();
        }
    }
}
