using GhCredentialProvider.Handlers;
using Newtonsoft.Json.Linq;
using NuGet.Protocol.Plugins;
using Xunit;

namespace GhCredentialProvider.Tests.Handlers;

public class GetOperationClaimsHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithGitHubPackageSource_ReturnsAuthenticationClaim()
    {
        var handler = new GetOperationClaimsHandler();
        var requestPayload = new GetOperationClaimsRequest(
            "https://nuget.pkg.github.com/owner/index.json",
            JObject.FromObject(
                new { PackageSourceRepository = "https://nuget.pkg.github.com/owner/index.json" }
            )
        );
        var payloadJson = JObject.FromObject(requestPayload);
        var request = new Message(
            "claims-789",
            MessageType.Request,
            MessageMethod.GetOperationClaims,
            payloadJson
        );

        var response = await handler.HandleAsync(request);

        Assert.NotNull(response);
        Assert.Equal(MessageType.Response, response.Type);
        var responsePayload = MessageUtilities.DeserializePayload<GetOperationClaimsResponse>(
            response
        );
        Assert.NotNull(responsePayload);
        Assert.Single(responsePayload.Claims);
        Assert.Equal(OperationClaim.Authentication, responsePayload.Claims.First());
    }

    [Fact]
    public async Task HandleAsync_WithNonGitHubPackageSource_ReturnsEmptyClaims()
    {
        var handler = new GetOperationClaimsHandler();
        var requestPayload = new GetOperationClaimsRequest(
            "https://api.nuget.org/v3/index.json",
            JObject.FromObject(
                new { PackageSourceRepository = "https://api.nuget.org/v3/index.json" }
            )
        );
        var payloadJson = JObject.FromObject(requestPayload);
        var request = new Message(
            "claims-789",
            MessageType.Request,
            MessageMethod.GetOperationClaims,
            payloadJson
        );

        var response = await handler.HandleAsync(request);

        Assert.NotNull(response);
        Assert.Equal(MessageType.Response, response.Type);
        var responsePayload = MessageUtilities.DeserializePayload<GetOperationClaimsResponse>(
            response
        );
        Assert.NotNull(responsePayload);
        Assert.Empty(responsePayload.Claims);
    }

    [Fact]
    public async Task HandleAsync_WithGitHubEnterprise_ReturnsAuthenticationClaim()
    {
        var handler = new GetOperationClaimsHandler();
        var requestPayload = new GetOperationClaimsRequest(
            "https://ghe.company.com/nuget/index.json",
            JObject.FromObject(
                new { PackageSourceRepository = "https://ghe.company.com/nuget/index.json" }
            )
        );
        var payloadJson = JObject.FromObject(requestPayload);
        var request = new Message(
            "claims-789",
            MessageType.Request,
            MessageMethod.GetOperationClaims,
            payloadJson
        );

        var response = await handler.HandleAsync(request);

        Assert.NotNull(response);
        Assert.Equal(MessageType.Response, response.Type);
        var responsePayload = MessageUtilities.DeserializePayload<GetOperationClaimsResponse>(
            response
        );
        Assert.NotNull(responsePayload);
        Assert.Single(responsePayload.Claims);
        Assert.Equal(OperationClaim.Authentication, responsePayload.Claims.First());
    }

    [Fact]
    public async Task HandleAsync_WithNullServiceIndexButGitHubPackageSource_ReturnsAuthenticationClaim()
    {
        var handler = new GetOperationClaimsHandler();
        var requestPayload = new GetOperationClaimsRequest(
            "https://nuget.pkg.github.com/owner/index.json",
            null
        );
        var payloadJson = JObject.FromObject(requestPayload);
        var request = new Message(
            "claims-789",
            MessageType.Request,
            MessageMethod.GetOperationClaims,
            payloadJson
        );

        var response = await handler.HandleAsync(request);

        Assert.NotNull(response);
        Assert.Equal(MessageType.Response, response.Type);
        var responsePayload = MessageUtilities.DeserializePayload<GetOperationClaimsResponse>(
            response
        );
        Assert.NotNull(responsePayload);
        Assert.Single(responsePayload.Claims);
        Assert.Equal(OperationClaim.Authentication, responsePayload.Claims.First());
    }
}
