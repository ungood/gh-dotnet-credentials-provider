using GithubCredentialProvider.GitHub;
using Xunit;

namespace GithubCredentialProvider.Tests.GitHub;

public class GitHubHostDetectorTests
{
    [Theory]
    [InlineData("https://nuget.pkg.github.com/owner/index.json", true)]
    [InlineData("https://nuget.pkg.github.com/myorg/index.json", true)]
    [InlineData("https://npm.pkg.github.com/owner/index.json", true)]
    [InlineData("https://github.com/owner/repo", false)]
    [InlineData("https://nuget.org/v3/index.json", false)]
    [InlineData("https://api.nuget.org/v3/index.json", false)]
    [InlineData("https://ghe.company.com/nuget/index.json", true)]
    [InlineData("https://github.company.com/nuget/index.json", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("not-a-uri", false)]
    public void IsGitHubHost_DetectsGitHubHosts(string? uri, bool expected)
    {
        var result = GitHubHostDetector.IsGitHubHost(uri);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("https://nuget.pkg.github.com/owner/index.json", "github.com")]
    [InlineData("https://npm.pkg.github.com/owner/index.json", "github.com")]
    [InlineData("https://ghe.company.com/nuget/index.json", "ghe.company.com")]
    [InlineData("https://github.company.com/nuget/index.json", "github.company.com")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("not-a-uri", null)]
    public void ExtractHostname_ExtractsCorrectHostname(string? uri, string? expected)
    {
        var result = GitHubHostDetector.ExtractHostname(uri);
        Assert.Equal(expected, result);
    }
}