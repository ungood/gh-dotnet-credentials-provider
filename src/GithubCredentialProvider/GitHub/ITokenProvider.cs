namespace GithubCredentialProvider.GitHub;

public interface ITokenProvider
{
    Task<string?> GetTokenAsync(string hostname, CancellationToken cancellationToken = default);
}