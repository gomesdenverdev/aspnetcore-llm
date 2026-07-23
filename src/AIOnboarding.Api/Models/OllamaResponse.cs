using System.Text.Json.Serialization;

namespace AIOnboarding.Api.Models;

public sealed class OllamaResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; } = default!;
}