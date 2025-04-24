using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarbonWise.BuildingBlocks.Application.Services.LLMService
{
    public class LlmRequest
    {
        [Required]
        public string Prompt { get; set; }

        public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();
    }

    public class LlmResponse
    {
        public string Content { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string Model { get; set; }
        public int TokensUsed { get; set; }
    }

    public class GroqChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("messages")]
        public List<GroqChatMessage> Messages { get; set; } = new List<GroqChatMessage>();

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }
    }

    public class GroqChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        public GroqChatMessage(string role, string content)
        {
            Role = role;
            Content = content;
        }
    }

    public class GroqChatResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("choices")]
        public List<GroqChatChoice> Choices { get; set; }

        [JsonPropertyName("usage")]
        public GroqUsage Usage { get; set; }
    }

    public class GroqChatChoice
    {
        [JsonPropertyName("message")]
        public GroqChatMessage Message { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; }
    }

    public class GroqUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}