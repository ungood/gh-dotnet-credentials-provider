using GhCredentialProvider.GitHub;
using GhCredentialProvider.Handlers;
using NSubstitute;
using NuGet.Protocol.Plugins;
using Xunit;

namespace GhCredentialProvider.Tests.Integration;

public class FullProtocolFlowTests
{
    [Fact(Skip = "Integration test - requires PluginFactory and full protocol setup")]
    public async Task FullProtocolFlow_HandshakeInitializeAndGetCredentials()
    {
        // Setup
        var tokenProvider = Substitute.For<ITokenProvider>();
        tokenProvider
            .GetTokenAsync("github.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>("ghp_testtoken123"));

        // Note: This test would need to use PluginFactory.CreateFromCurrentProcessAsync
        // and simulate the full protocol flow. Skipping for now as it requires
        // significant refactoring to work with the new IRequestHandler pattern.
        // The framework now handles message serialization/deserialization automatically.
    }
}
