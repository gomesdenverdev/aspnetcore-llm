using System.Text.Json.Serialization;

namespace AIOnboarding.Api.Models;

public sealed class OllamaOptions
{
    [JsonPropertyName("temperature")]
    public decimal Temperature { get; set; }

    [JsonPropertyName("instruction")]
    public string? Instruction { get; set; }
}