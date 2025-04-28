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


    public class ExternalAPIsSettings
    {
        public string ApiToken { get; set; }
        public string ApiBaseUrl { get; set; } = "https://api.waqi.info";
    }

    public class ExternalAPIsService : IExternalAPIsService
    {
        private readonly HttpClient _httpClient;
        private readonly ExternalAPIsSettings _settings;
        private readonly JsonSerializerOptions _jsonOptions;

        public ExternalAPIsService(IOptions<ExternalAPIsSettings> settings, HttpClient httpClient = null)
        {
            _settings = settings.Value;
            _httpClient = httpClient ?? new HttpClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<AirQualityResponse> GetAirQualityDataAsync(string location)
        {
            var requestUrl = $"{_settings.ApiBaseUrl}/feed/{location}/?token={_settings.ApiToken}";
            return await ExecuteRequestAsync(requestUrl);
        }

        public async Task<AirQualityResponse> GetAirQualityByGeoLocationAsync(double latitude, double longitude)
        {
            var requestUrl = $"{_settings.ApiBaseUrl}/feed/geo:{latitude};{longitude}/?token={_settings.ApiToken}";
            return await ExecuteRequestAsync(requestUrl);
        }

        private async Task<AirQualityResponse> ExecuteRequestAsync(string requestUrl)
        {
            try
            {
                var response = await _httpClient.GetAsync(requestUrl);

                var responseContent = await response.Content.ReadAsStringAsync();

                // Debug
                Console.WriteLine($"API Response: {responseContent}");

                // First, parse to a dynamic structure to check the status
                using (JsonDocument document = JsonDocument.Parse(responseContent))
                {
                    var root = document.RootElement;

                    // Create a basic response with the status
                    var result = new AirQualityResponse
                    {
                        Status = root.GetProperty("status").GetString()
                    };

                    // If status is "ok" and data isn't null, try to parse the data
                    if (result.Status == "ok" && root.TryGetProperty("data", out var dataElement) && dataElement.ValueKind != JsonValueKind.Null)
                    {
                        result.Data = JsonSerializer.Deserialize<AirQualityData>(dataElement.GetRawText(), _jsonOptions);
                    }
                    else if (root.TryGetProperty("data", out var errorElement))
                    {
                        // If data is a string, it's probably an error message
                        if (errorElement.ValueKind == JsonValueKind.String)
                        {
                            result.ErrorMessage = errorElement.GetString();
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                return new AirQualityResponse
                {
                    Status = "error",
                    ErrorMessage = $"Exception while processing request: {ex.Message}"
                };
            }
        }
    }

    // Response models for air quality API
    public class AirQualityResponse
    {
        public string Status { get; set; }
        public AirQualityData Data { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class AirQualityData
    {
        public int Aqi { get; set; }
        public int Idx { get; set; }

        [JsonPropertyName("attributions")]
        public List<Attribution> Attributions { get; set; }

        [JsonPropertyName("city")]
        public City City { get; set; }

        [JsonPropertyName("dominentpol")]
        public string DominentPol { get; set; }

        [JsonPropertyName("iaqi")]
        public Iaqi Iaqi { get; set; }

        [JsonPropertyName("time")]
        public Time Time { get; set; }

        [JsonPropertyName("forecast")]
        public Forecast Forecast { get; set; }

        [JsonPropertyName("debug")]
        public Debug Debug { get; set; }
    }

    public class Attribution
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class City
    {
        [JsonPropertyName("geo")]
        public List<double> Geo { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }
    }

    public class Iaqi
    {
        [JsonPropertyName("co")]
        public Measurement Co { get; set; }

        [JsonPropertyName("h")]
        public Measurement Humidity { get; set; }

        [JsonPropertyName("no2")]
        public Measurement No2 { get; set; }

        [JsonPropertyName("o3")]
        public Measurement Ozone { get; set; }

        [JsonPropertyName("p")]
        public Measurement Pressure { get; set; }

        [JsonPropertyName("pm10")]
        public Measurement Pm10 { get; set; }

        [JsonPropertyName("pm25")]
        public Measurement Pm25 { get; set; }

        [JsonPropertyName("so2")]
        public Measurement So2 { get; set; }

        [JsonPropertyName("t")]
        public Measurement Temperature { get; set; }

        [JsonPropertyName("w")]
        public Measurement WindSpeed { get; set; }
    }

    public class Measurement
    {
        [JsonPropertyName("v")]
        public double V { get; set; }
    }

    public class Time
    {
        [JsonPropertyName("s")]
        public string S { get; set; }

        [JsonPropertyName("tz")]
        public string Tz { get; set; }

        [JsonPropertyName("v")]
        public long V { get; set; }

        [JsonPropertyName("iso")]
        public string Iso { get; set; }
    }

    public class Forecast
    {
        [JsonPropertyName("daily")]
        public Daily Daily { get; set; }
    }

    public class Daily
    {
        [JsonPropertyName("o3")]
        public List<PollutantForecast> O3 { get; set; }

        [JsonPropertyName("pm10")]
        public List<PollutantForecast> Pm10 { get; set; }

        [JsonPropertyName("pm25")]
        public List<PollutantForecast> Pm25 { get; set; }

        [JsonPropertyName("uvi")]
        public List<PollutantForecast> Uvi { get; set; }
    }

    public class PollutantForecast
    {
        [JsonPropertyName("avg")]
        public int Avg { get; set; }

        [JsonPropertyName("day")]
        public string Day { get; set; }

        [JsonPropertyName("max")]
        public int Max { get; set; }

        [JsonPropertyName("min")]
        public int Min { get; set; }
    }

    public class Debug
    {
        [JsonPropertyName("sync")]
        public string Sync { get; set; }
    }
}