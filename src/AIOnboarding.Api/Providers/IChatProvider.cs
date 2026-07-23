namespace AIOnboarding.Api.Providers;

public interface IChatProvider
{
    Task<string> AskAsync(string prompt);
}