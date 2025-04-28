using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace CarbonWise.BuildingBlocks.Application.Services.ExternalAPIs
{
    public interface IExternalAPIsService
    {
        Task<AirQualityResponse> GetAirQualityDataAsync(string location);
        Task<AirQualityResponse> GetAirQualityByGeoLocationAsync(double latitude, double longitude);
    }



  
}