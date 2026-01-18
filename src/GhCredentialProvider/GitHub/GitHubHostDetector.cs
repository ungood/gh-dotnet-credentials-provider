namespace GhCredentialProvider.GitHub;

public static class GitHubHostDetector
{
    private static readonly string[] GitHubPackageHosts =
    [
        "nuget.pkg.github.com",
        "npm.pkg.github.com"
    ];

    public static bool IsGitHubHost(string? uri)
    {
        if (string.IsNullOrWhiteSpace(uri)) return false;

        try
        {
            if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri)) return false;

            var host = parsedUri.Host.ToLowerInvariant();

            // Check for standard GitHub Packages hosts
            if (GitHubPackageHosts.Any(ghHost => host.Contains(ghHost, StringComparison.Ordinal))) return true;

            // Check for GitHub Enterprise (custom domain)
            // GitHub Enterprise typically has a pattern like ghe.company.com or github.company.com
            // We'll check if it contains "ghe" in the hostname (but not just "github.com")
            if (host.Contains("ghe", StringComparison.Ordinal) || (host.Contains("github", StringComparison.Ordinal) && !host.Equals("github.com", StringComparison.Ordinal)))
                // Additional check: if it's a known GitHub domain pattern
                return true;

            return false;
        }
        catch
        {
            return false;
        }
    }

    public static string? ExtractHostname(string? uri)
    {
        if (string.IsNullOrWhiteSpace(uri)) return null;

        try
        {
            if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri)) return null;

            var host = parsedUri.Host.ToLowerInvariant();

            // For standard GitHub, return "github.com"
            if (host.Contains("nuget.pkg.github.com", StringComparison.Ordinal) || host.Contains("npm.pkg.github.com", StringComparison.Ordinal)) return "github.com";

            // For GitHub Enterprise, extract the base domain
            // This is a simplified approach - in practice, you might need more sophisticated logic
            return host;
        }
        catch
        {
            return null;
        }
    }
}