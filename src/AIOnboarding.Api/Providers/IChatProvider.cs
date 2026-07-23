namespace AIOnboarding.Api.Providers;

public interface IChatProvider
{
    IAsyncEnumerable<string> AskAsync(string prompt, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> ChatAsync(string prompt, CancellationToken cancellationToken = default);
}