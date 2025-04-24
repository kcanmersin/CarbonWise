using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;
using CarbonWise.BuildingBlocks.Application.Services.LLMService;

namespace CarbonWise.BuildingBlocks.Application.Services.LLMService
{
    public class LlmSettings
    {
        public string GroqApiKey { get; set; }
        public string GroqApiUrl { get; set; } = "https://api.groq.com/openai/v1/chat/completions";
        public string Model { get; set; } = "llama-3.3-70b-versatile";
        public double Temperature { get; set; } = 0.7;
        public int MaxTokens { get; set; } = 2048;
    }

    public class LlmService : ILlmService
    {
        private readonly HttpClient _httpClient;
        private readonly LlmSettings _settings;
        private readonly JsonSerializerOptions _jsonOptions;

        public LlmService(IOptions<LlmSettings> settings, HttpClient httpClient = null)
        {
            _settings = settings.Value;
            _httpClient = httpClient ?? new HttpClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<LlmResponse> GenerateContentAsync(LlmRequest request)
        {
            var groqRequest = new GroqChatRequest
            {
                Model = _settings.Model,
                Temperature = _settings.Temperature,
                MaxTokens = _settings.MaxTokens,
                Messages = new List<GroqChatMessage>
                {
                    new GroqChatMessage("user", request.Prompt)
                }
            };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(groqRequest, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.GroqApiKey}");

            var response = await _httpClient.PostAsync(_settings.GroqApiUrl, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to get response from LLM API. Status code: {response.StatusCode}, Error: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var groqResponse = JsonSerializer.Deserialize<GroqChatResponse>(responseContent, _jsonOptions);

            return new LlmResponse
            {
                Content = groqResponse.Choices[0].Message.Content,
                Model = groqResponse.Model,
                TokensUsed = groqResponse.Usage.TotalTokens
            };
        }
    }
}