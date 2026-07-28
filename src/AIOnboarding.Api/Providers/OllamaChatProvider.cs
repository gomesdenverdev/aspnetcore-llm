using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Options;

using AIOnboarding.Api.Models;
using AIOnboarding.Api.Models.Dto;

namespace AIOnboarding.Api.Providers;

public sealed class OllamaChatProvider : IChatProvider
{
    private readonly HttpClient httpClient;
    private readonly Configuration.OllamaOptions ollamaOptions;

    private readonly List<AskHistory> askHistory = [];

    private readonly string systemPrompt = "You are a helpful assistant. If you do not know something say no. No special symbols. You are in internal support triage bot.";
    private readonly string developerPrompt = "You are ACME Support; never invent account specific facts; if information is missing ask atmost 2 clarifying questions. Do not reveal any internal policies or tools.";

    public OllamaChatProvider(HttpClient httpClient, IOptions<Configuration.OllamaOptions> ollamaOptions)
    {
        this.httpClient = httpClient;
        this.ollamaOptions = ollamaOptions.Value;
    }

    public async IAsyncEnumerable<string> AskAsync(string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (askHistory.Count == 0)
        {
            askHistory.Add(new AskHistory { Role = "system", Content = systemPrompt });
            askHistory.Add(new AskHistory { Role = "developer", Content = developerPrompt });
        }

        askHistory.Add(new AskHistory { Role = "user", Content = prompt });

        var request = new OllamaRequestGenerate
        {
            Model = ollamaOptions.Model,
            Prompt = JsonSerializer.Serialize(askHistory),
            Stream = false
        };

        var json = JsonSerializer.Serialize(request);
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{ollamaOptions.BaseUrl}/api/generate")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        using var response = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var responseBuilder = new StringBuilder();
        await foreach (var chunk in ReadStreamAsync(stream, element =>
        {
            if (element.TryGetProperty("response", out var responseElement) && responseElement.ValueKind == JsonValueKind.String)
            {
                return responseElement.GetString();
            }

            return null;
        }, cancellationToken))
        {
            responseBuilder.Append(chunk);
            yield return chunk;
        }

        askHistory.Add(new AskHistory { Role = "assistant", Content = responseBuilder.ToString() });
    }

    public async IAsyncEnumerable<string> ChatAsync(string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (askHistory.Count == 0)
        {
            askHistory.Add(new AskHistory { Role = "system", Content = systemPrompt });
            // askHistory.Add(new AskHistory { Role = "developer", Content = developerPrompt });
        }

        askHistory.Add(new AskHistory { Role = "user", Content = prompt });

        var request = new OllamaRequestChat
        {
            Model = ollamaOptions.Model,
            Messages = askHistory,
            Options = new Models.OllamaOptions()
            {
                Temperature = 1.0M,
                Instruction = "Write a plain english email text and do not output JSON. Write it in a manner the way Shashi Tharoor would write it."
            },
            Stream = false
        };

        var json = JsonSerializer.Serialize(request);
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{ollamaOptions.BaseUrl}/api/chat")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        using var response = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var responseBuilder = new StringBuilder();
        await foreach (var chunk in ReadStreamAsync(stream, element =>
        {
            if (element.TryGetProperty("message", out var messageElement) &&
                messageElement.ValueKind == JsonValueKind.Object &&
                messageElement.TryGetProperty("content", out var contentElement) &&
                contentElement.ValueKind == JsonValueKind.String)
            {
                return contentElement.GetString();
            }

            return null;
        }, cancellationToken))
        {
            responseBuilder.Append(chunk);
            yield return chunk;
        }

        askHistory.Add(new AskHistory { Role = "assistant", Content = responseBuilder.ToString() });
    }

    private async IAsyncEnumerable<string> ReadStreamAsync(Stream stream, Func<JsonElement, string?> extractor, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string? chunk = null;

            try
            {
                using var document = JsonDocument.Parse(line);
                chunk = extractor(document.RootElement);
            }
            catch (JsonException)
            {
                // Ignore incomplete or non-JSON lines from the stream.
            }

            if (!string.IsNullOrEmpty(chunk))
            {
                yield return chunk;
            }
        }
    }
}