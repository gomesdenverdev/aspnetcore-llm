using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Options;

using AIOnboarding.Api.Configuration;
using AIOnboarding.Api.Models;
using AIOnboarding.Api.Models.Dto;

namespace AIOnboarding.Api.Providers;

public sealed class OllamaChatProvider : IChatProvider
{
    private readonly HttpClient httpClient;
    private readonly OllamaOptions ollamaOptions;

    private List<AskHistory> askHistory = []; 

    public OllamaChatProvider(HttpClient httpClient, IOptions<OllamaOptions> ollamaOptions)
    {
        this.httpClient = httpClient;
        this.ollamaOptions = ollamaOptions.Value;
    }

    public async Task<string> AskAsync(string prompt)
    {
        if (askHistory.Count == 0)
        {
            askHistory.Add(new AskHistory { Role = "system", Content = "You are a helpful assistant. If you do not know something say no. No special symbols." });
        }
        askHistory.Add(new AskHistory { Role = "user", Content = prompt });

        var request = new OllamaRequest
        {
            Model = ollamaOptions.Model,
            Prompt = JsonSerializer.Serialize(askHistory),
            Stream = false
        };

        var json = JsonSerializer.Serialize(request);

        using var response = await httpClient.PostAsync($"{ollamaOptions.BaseUrl}/api/generate", new StringContent(json, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(content);

        var result = ollamaResponse?.Response ?? string.Empty;

        askHistory.Add(new AskHistory { Role = "assistant", Content = result });

        return result;
    }
}