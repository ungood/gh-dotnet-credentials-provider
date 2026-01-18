using GhCredentialProvider.GitHub;
using GhCredentialProvider.RequestHandlers;
using NSubstitute;
using NuGet.Protocol.Plugins;
using Xunit;

namespace GhCredentialProvider.Tests.Handlers;

public class GetAuthenticationCredentialsHandlerTests
{
    [Fact]
    public async Task HandleRequestAsync_WithValidToken_ReturnsSuccess()
    {
        var tokenProvider = Substitute.For<ITokenProvider>();
        tokenProvider
            .GetTokenAsync("github.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>("ghp_testtoken123"));

        var handler = new GetAuthenticationCredentialsHandler(tokenProvider);
        var request = new GetAuthenticationCredentialsRequest(
            new Uri("https://nuget.pkg.github.com/owner/index.json"),
            false,
            true,
            false
        );

        var response = await handler.HandleRequestAsync(request);

        Assert.NotNull(response);
        Assert.Equal("USERNAME", response.Username);
        Assert.Equal("ghp_testtoken123", response.Password);
        Assert.Null(response.Message);
        Assert.Single(response.AuthenticationTypes);
        Assert.Equal("basic", response.AuthenticationTypes.First());
        Assert.Equal(MessageResponseCode.Success, response.ResponseCode);
    }

    [Fact]
    public async Task HandleRequestAsync_WithNoTokenAndNonInteractive_ReturnsNotFound()
    {
        var tokenProvider = Substitute.For<ITokenProvider>();
        tokenProvider
            .GetTokenAsync("github.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>(null));

        var handler = new GetAuthenticationCredentialsHandler(tokenProvider);
        var request = new GetAuthenticationCredentialsRequest(
            new Uri("https://nuget.pkg.github.com/owner/index.json"),
            false,
            true,
            false
        );

        var response = await handler.HandleRequestAsync(request);

        Assert.NotNull(response);
        Assert.Null(response.Username);
        Assert.Null(response.Password);
        Assert.Equal("No GitHub token available", response.Message);
        Assert.Equal(MessageResponseCode.NotFound, response.ResponseCode);
    }

    [Fact]
    public async Task HandleRequestAsync_WithNoTokenAndInteractive_ReturnsNotFound()
    {
        var tokenProvider = Substitute.For<ITokenProvider>();
        tokenProvider
            .GetTokenAsync("github.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>(null));

        var handler = new GetAuthenticationCredentialsHandler(tokenProvider);
        var request = new GetAuthenticationCredentialsRequest(
            new Uri("https://nuget.pkg.github.com/owner/index.json"),
            false,
            false,
            true
        );

        var response = await handler.HandleRequestAsync(request);

        Assert.NotNull(response);
        Assert.Equal("No GitHub token available", response.Message);
        Assert.Equal(MessageResponseCode.NotFound, response.ResponseCode);
    }

    [Fact]
    public async Task HandleRequestAsync_WithNonGitHubUri_ReturnsNotFound()
    {
        var tokenProvider = Substitute.For<ITokenProvider>();
        var handler = new GetAuthenticationCredentialsHandler(tokenProvider);
        var request = new GetAuthenticationCredentialsRequest(
            new Uri("https://api.nuget.org/v3/index.json"),
            false,
            true,
            false
        );

        var response = await handler.HandleRequestAsync(request);

        Assert.NotNull(response);
        Assert.Null(response.Message);
        Assert.Equal(MessageResponseCode.NotFound, response.ResponseCode);
    }

    [Fact]
    public async Task HandleRequestAsync_WithGitHubEnterprise_ExtractsCorrectHostname()
    {
        var tokenProvider = Substitute.For<ITokenProvider>();
        tokenProvider
            .GetTokenAsync("ghe.company.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>("ghp_enterprise_token"));

        var handler = new GetAuthenticationCredentialsHandler(tokenProvider);
        var request = new GetAuthenticationCredentialsRequest(
            new Uri("https://ghe.company.com/nuget/index.json"),
            false,
            true,
            false
        );

        var response = await handler.HandleRequestAsync(request);

        Assert.NotNull(response);
        Assert.Equal("ghp_enterprise_token", response.Password);
        await tokenProvider
            .Received(1)
            .GetTokenAsync("ghe.company.com", Arg.Any<CancellationToken>());
    }
}