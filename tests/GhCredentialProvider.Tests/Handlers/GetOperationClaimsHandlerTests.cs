using GhCredentialProvider.RequestHandlers;
using Newtonsoft.Json.Linq;
using NuGet.Protocol.Plugins;
using Xunit;

namespace GhCredentialProvider.Tests.Handlers;

public class GetOperationClaimsHandlerTests
{
    [Fact]
    public async Task HandleRequestAsync_WithGitHubPackageSource_ReturnsAuthenticationClaim()
    {
        var handler = new GetOperationClaimsHandler();
        var request = new GetOperationClaimsRequest(
            "https://nuget.pkg.github.com/owner/index.json",
            JObject.FromObject(
                new { PackageSourceRepository = "https://nuget.pkg.github.com/owner/index.json" }
            )
        );

        var response = await handler.HandleRequestAsync(request);

        Assert.NotNull(response);
        Assert.Single(response.Claims);
        Assert.Equal(OperationClaim.Authentication, response.Claims[0]);
    }

    [Fact]
    public async Task HandleRequestAsync_WithNonGitHubPackageSource_ReturnsEmptyClaims()
    {
        var handler = new GetOperationClaimsHandler();
        var request = new GetOperationClaimsRequest(
            "https://api.nuget.org/v3/index.json",
            JObject.FromObject(
                new { PackageSourceRepository = "https://api.nuget.org/v3/index.json" }
            )
        );

        var response = await handler.HandleRequestAsync(request);

        Assert.NotNull(response);
        Assert.Empty(response.Claims);
    }

    [Fact]
    public async Task HandleRequestAsync_WithGitHubEnterprise_ReturnsAuthenticationClaim()
    {
        var handler = new GetOperationClaimsHandler();
        var request = new GetOperationClaimsRequest(
            "https://ghe.company.com/nuget/index.json",
            JObject.FromObject(
                new { PackageSourceRepository = "https://ghe.company.com/nuget/index.json" }
            )
        );

        var response = await handler.HandleRequestAsync(request);

        Assert.NotNull(response);
        Assert.Single(response.Claims);
        Assert.Equal(OperationClaim.Authentication, response.Claims[0]);
    }

    [Fact]
    public async Task HandleRequestAsync_WithNullServiceIndexButGitHubPackageSource_ReturnsAuthenticationClaim()
    {
        var handler = new GetOperationClaimsHandler();
        var request = new GetOperationClaimsRequest(
            "https://nuget.pkg.github.com/owner/index.json",
            null
        );

        var response = await handler.HandleRequestAsync(request);

        Assert.NotNull(response);
        Assert.Single(response.Claims);
        Assert.Equal(OperationClaim.Authentication, response.Claims[0]);
    }
}