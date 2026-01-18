using System.Diagnostics;
using GhCredentialProvider.Logging;
using NuGet.Common;

namespace GhCredentialProvider.GitHub;

public class GitHubCliTokenProvider : ITokenProvider
{
    private readonly StandardErrorLogger _logger;

    public GitHubCliTokenProvider()
    {
        _logger = new StandardErrorLogger(nameof(GitHubCliTokenProvider));
    }

    public async Task<string?> GetTokenAsync(
        string hostname,
        CancellationToken cancellationToken = default
    )
    {
        _logger.Log(LogLevel.Debug, $"Attempting to retrieve token for hostname: {hostname}");

        // First, check environment variables
        var envToken =
            Environment.GetEnvironmentVariable("GH_TOKEN")
            ?? Environment.GetEnvironmentVariable("GITHUB_TOKEN");

        if (!string.IsNullOrWhiteSpace(envToken))
        {
            _logger.Log(
                LogLevel.Information,
                $"Token retrieved from environment variable for hostname: {hostname}"
            );
            return envToken.Trim();
        }

        _logger.Log(
            LogLevel.Debug,
            $"No token found in environment variables, attempting to retrieve from gh CLI"
        );

        // Try to get token from gh CLI
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "gh",
                Arguments = $"auth token --hostname {hostname}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = new Process { StartInfo = processStartInfo };
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                _logger.Log(
                    LogLevel.Information,
                    $"Token retrieved from gh CLI for hostname: {hostname}"
                );
                return output.Trim();
            }
            else
            {
                _logger.Log(
                    LogLevel.Warning,
                    $"gh CLI command failed with exit code {process.ExitCode} for hostname: {hostname}"
                );
            }
        }
        catch (Exception ex)
        {
            // gh CLI not available or failed
            _logger.Log(
                LogLevel.Warning,
                $"Failed to retrieve token from gh CLI for hostname {hostname}: {ex.Message}"
            );
        }

        _logger.Log(LogLevel.Warning, $"No token available for hostname: {hostname}");
        return null;
    }
}
