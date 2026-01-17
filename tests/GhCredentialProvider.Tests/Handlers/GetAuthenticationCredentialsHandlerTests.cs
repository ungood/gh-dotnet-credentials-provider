using GhCredentialProvider.GitHub;
using GhCredentialProvider.Handlers;
using Newtonsoft.Json.Linq;
using NuGet.Protocol.Plugins;
using NSubstitute;
using Xunit;

namespace GhCredentialProvider.Tests.Handlers;

public class GetAuthenticationCredentialsHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidToken_ReturnsSuccess()
    {
        var tokenProvider = Substitute.For<ITokenProvider>();
        tokenProvider.GetTokenAsync("github.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>("ghp_testtoken123"));

        var handler = new GetAuthenticationCredentialsHandler(tokenProvider);
        var requestPayload = new GetAuthenticationCredentialsRequest(
            new Uri("https://nuget.pkg.github.com/owner/index.json"),
            isRetry: false,
            isNonInteractive: true,
            canShowDialog: false);
        var payloadJson = JObject.FromObject(requestPayload);
        var request = new Message("auth-101", MessageType.Request, MessageMethod.GetAuthenticationCredentials, payloadJson);

        var response = await handler.HandleAsync(request);

        Assert.NotNull(response);
        Assert.Equal(MessageType.Response, response.Type);
        Assert.Equal("auth-101", response.RequestId);
        var responsePayload = MessageUtilities.DeserializePayload<GetAuthenticationCredentialsResponse>(response);
        Assert.NotNull(responsePayload);
        Assert.Equal("USERNAME", responsePayload.Username);
        Assert.Equal("ghp_testtoken123", responsePayload.Password);
        Assert.Equal("Credentials retrieved successfully", responsePayload.Message);
        Assert.Single(responsePayload.AuthenticationTypes);
        Assert.Equal("GitHubPAT", responsePayload.AuthenticationTypes.First());
        Assert.Equal(MessageResponseCode.Success, responsePayload.ResponseCode);
    }

    [Fact]
    public async Task HandleAsync_WithNoTokenAndNonInteractive_ReturnsError()
    {
        var tokenProvider = Substitute.For<ITokenProvider>();
        tokenProvider.GetTokenAsync("github.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>(null));

        var handler = new GetAuthenticationCredentialsHandler(tokenProvider);
        var requestPayload = new GetAuthenticationCredentialsRequest(
            new Uri("https://nuget.pkg.github.com/owner/index.json"),
            isRetry: false,
            isNonInteractive: true,
            canShowDialog: false);
        var payloadJson = JObject.FromObject(requestPayload);
        var request = new Message("auth-101", MessageType.Request, MessageMethod.GetAuthenticationCredentials, payloadJson);

        var response = await handler.HandleAsync(request);

        Assert.NotNull(response);
        Assert.Equal(MessageType.Response, response.Type);
        var responsePayload = MessageUtilities.DeserializePayload<GetAuthenticationCredentialsResponse>(response);
        Assert.NotNull(responsePayload);
        Assert.Null(responsePayload.Username);
        Assert.Null(responsePayload.Password);
        Assert.Contains("non-interactive", responsePayload.Message ?? "", StringComparison.OrdinalIgnoreCase);
        Assert.Equal(MessageResponseCode.Error, responsePayload.ResponseCode);
    }

    [Fact]
    public async Task HandleAsync_WithNoTokenAndInteractive_ReturnsError()
    {
        var tokenProvider = Substitute.For<ITokenProvider>();
        tokenProvider.GetTokenAsync("github.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>(null));

        var handler = new GetAuthenticationCredentialsHandler(tokenProvider);
        var requestPayload = new GetAuthenticationCredentialsRequest(
            new Uri("https://nuget.pkg.github.com/owner/index.json"),
            isRetry: false,
            isNonInteractive: false,
            canShowDialog: true);
        var payloadJson = JObject.FromObject(requestPayload);
        var request = new Message("auth-101", MessageType.Request, MessageMethod.GetAuthenticationCredentials, payloadJson);

        var response = await handler.HandleAsync(request);

        Assert.NotNull(response);
        Assert.Equal(MessageType.Response, response.Type);
        var responsePayload = MessageUtilities.DeserializePayload<GetAuthenticationCredentialsResponse>(response);
        Assert.NotNull(responsePayload);
        Assert.Contains("Unable to retrieve", responsePayload.Message ?? "");
        Assert.Equal(MessageResponseCode.Error, responsePayload.ResponseCode);
    }

    [Fact]
    public async Task HandleAsync_WithNonGitHubUri_ReturnsError()
    {
        var tokenProvider = Substitute.For<ITokenProvider>();
        var handler = new GetAuthenticationCredentialsHandler(tokenProvider);
        var requestPayload = new GetAuthenticationCredentialsRequest(
            new Uri("https://api.nuget.org/v3/index.json"),
            isRetry: false,
            isNonInteractive: true,
            canShowDialog: false);
        var payloadJson = JObject.FromObject(requestPayload);
        var request = new Message("auth-101", MessageType.Request, MessageMethod.GetAuthenticationCredentials, payloadJson);

        var response = await handler.HandleAsync(request);

        Assert.NotNull(response);
        Assert.Equal(MessageType.Response, response.Type);
        var responsePayload = MessageUtilities.DeserializePayload<GetAuthenticationCredentialsResponse>(response);
        Assert.NotNull(responsePayload);
        Assert.Equal("Not a GitHub package source", responsePayload.Message);
        Assert.Equal(MessageResponseCode.Error, responsePayload.ResponseCode);
    }

    [Fact]
    public async Task HandleAsync_WithGitHubEnterprise_ExtractsCorrectHostname()
    {
        var tokenProvider = Substitute.For<ITokenProvider>();
        tokenProvider.GetTokenAsync("ghe.company.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>("ghp_enterprise_token"));

        var handler = new GetAuthenticationCredentialsHandler(tokenProvider);
        var requestPayload = new GetAuthenticationCredentialsRequest(
            new Uri("https://ghe.company.com/nuget/index.json"),
            isRetry: false,
            isNonInteractive: true,
            canShowDialog: false);
        var payloadJson = JObject.FromObject(requestPayload);
        var request = new Message("auth-101", MessageType.Request, MessageMethod.GetAuthenticationCredentials, payloadJson);

        var response = await handler.HandleAsync(request);

        Assert.NotNull(response);
        Assert.Equal(MessageType.Response, response.Type);
        var responsePayload = MessageUtilities.DeserializePayload<GetAuthenticationCredentialsResponse>(response);
        Assert.NotNull(responsePayload);
        Assert.Equal("ghp_enterprise_token", responsePayload.Password);
        await tokenProvider.Received(1).GetTokenAsync("ghe.company.com", Arg.Any<CancellationToken>());
    }
}
