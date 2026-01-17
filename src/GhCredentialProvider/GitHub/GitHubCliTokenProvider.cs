using System.Diagnostics;

namespace GhCredentialProvider.GitHub;

public class GitHubCliTokenProvider : ITokenProvider
{
    public async Task<string?> GetTokenAsync(
        string hostname,
        CancellationToken cancellationToken = default
    )
    {
        // First, check environment variables
        var envToken =
            Environment.GetEnvironmentVariable("GH_TOKEN")
            ?? Environment.GetEnvironmentVariable("GITHUB_TOKEN");

        if (!string.IsNullOrWhiteSpace(envToken))
        {
            return envToken.Trim();
        }

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
                return output.Trim();
            }
        }
        catch
        {
            // gh CLI not available or failed
        }

        return null;
    }
}
