using System.Text.Json.Serialization;

namespace AIOnboarding.Api.Models;

public sealed class Message
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = default!;

    [JsonPropertyName("content")]
    public string Content { get; set; } = default!;
}

public sealed class OllamaChatResponse
{
    [JsonPropertyName("message")]
    public Message Message { get; set; } = default!;
}