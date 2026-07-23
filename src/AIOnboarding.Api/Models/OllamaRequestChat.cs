using System.Text.Json.Serialization;

using AIOnboarding.Api.Models.Dto;

namespace AIOnboarding.Api.Models;

public sealed class OllamaRequestChat
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;

    [JsonPropertyName("messages")]
    public List<AskHistory> Messages { get; set; } = default!;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("options")]
    public OllamaOptions? Options { get; set; }
}
