using System.Text.Json.Serialization;

namespace AIOnboarding.Api.Models;

public sealed class OllamaRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = default!;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = default!;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
}