using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Options;

using AIOnboarding.Api.Configuration;
using AIOnboarding.Api.Models;

namespace AIOnboarding.Api.Providers;

public sealed class OllamaChatProvider : IChatProvider
{
    private readonly HttpClient httpClient;
    private readonly OllamaOptions ollamaOptions;

    public OllamaChatProvider(HttpClient httpClient, IOptions<OllamaOptions> ollamaOptions)
    {
        this.httpClient = httpClient;
        this.ollamaOptions = ollamaOptions.Value;
    }

    public async Task<string> AskAsync(string prompt)
    {
        var systemInstructions = "You are a helpful assistant. If you don't know something say no. No special symbols.";
        var refinedPrompt = $"System: {systemInstructions}\nUser: {prompt}";

        var request = new OllamaRequest
        {
            Model = ollamaOptions.Model,
            Prompt = refinedPrompt,
            Stream = false
        };

        var json = JsonSerializer.Serialize(request);

        using var response = await httpClient.PostAsync($"{ollamaOptions.BaseUrl}/api/generate", new StringContent(json, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(content);

        return ollamaResponse?.Response ?? string.Empty;
    }
}