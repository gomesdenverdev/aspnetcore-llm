namespace AIOnboarding.Api.Providers;

public interface IChatProvider
{
    Task<string> AskAsync(string prompt);
    Task<string> ChatAsync(string prompt);
}