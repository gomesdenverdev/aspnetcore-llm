using System.Text.Json.Serialization;

namespace AIOnboarding.Api.Models.Dto;

public sealed class AskHistory
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = default!;

    [JsonPropertyName("content")]
    public string Content { get; set; } = default!;
}