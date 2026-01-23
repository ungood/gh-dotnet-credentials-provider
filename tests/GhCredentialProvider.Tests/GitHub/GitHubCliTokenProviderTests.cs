using GithubCredentialProvider.GitHub;
using Xunit;

namespace GithubCredentialProvider.Tests.GitHub;

public class GitHubCliTokenProviderTests
{
    [Fact]
    public async Task GetTokenAsync_WithEnvironmentVariable_ReturnsToken()
    {
        var originalToken = Environment.GetEnvironmentVariable("GH_TOKEN");
        try
        {
            Environment.SetEnvironmentVariable("GH_TOKEN", "ghp_testtoken123");
            var provider = new GitHubCliTokenProvider();

            var token = await provider.GetTokenAsync("github.com");

            Assert.Equal("ghp_testtoken123", token);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GH_TOKEN", originalToken);
        }
    }

    [Fact]
    public async Task GetTokenAsync_WithGithubTokenEnvironmentVariable_ReturnsToken()
    {
        var originalToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        try
        {
            Environment.SetEnvironmentVariable("GITHUB_TOKEN", "ghp_testtoken456");
            var provider = new GitHubCliTokenProvider();

            var token = await provider.GetTokenAsync("github.com");

            Assert.Equal("ghp_testtoken456", token);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_TOKEN", originalToken);
        }
    }

    [Fact]
    public async Task GetTokenAsync_WithGhTokenPrecedence_ReturnsGhToken()
    {
        var originalGhToken = Environment.GetEnvironmentVariable("GH_TOKEN");
        var originalGithubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        try
        {
            Environment.SetEnvironmentVariable("GH_TOKEN", "ghp_gh_token");
            Environment.SetEnvironmentVariable("GITHUB_TOKEN", "ghp_github_token");
            var provider = new GitHubCliTokenProvider();

            var token = await provider.GetTokenAsync("github.com");

            // GH_TOKEN should take precedence
            Assert.Equal("ghp_gh_token", token);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GH_TOKEN", originalGhToken);
            Environment.SetEnvironmentVariable("GITHUB_TOKEN", originalGithubToken);
        }
    }

    [Fact]
    public async Task GetTokenAsync_TrimsWhitespace()
    {
        var originalToken = Environment.GetEnvironmentVariable("GH_TOKEN");
        try
        {
            Environment.SetEnvironmentVariable("GH_TOKEN", "  ghp_testtoken  \n");
            var provider = new GitHubCliTokenProvider();

            var token = await provider.GetTokenAsync("github.com");

            Assert.Equal("ghp_testtoken", token);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GH_TOKEN", originalToken);
        }
    }
}