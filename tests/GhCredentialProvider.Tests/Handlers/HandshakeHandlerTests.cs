using GhCredentialProvider.RequestHandlers;
using NuGet.Protocol.Plugins;
using NuGet.Versioning;
using Xunit;

namespace GhCredentialProvider.Tests.Handlers;

public class HandshakeHandlerTests
{
    [Fact]
    public async Task HandleRequestAsync_WithValidProtocolVersion_ReturnsSuccess()
    {
        var handler = new HandshakeRequestHandler();
        var request = new HandshakeRequest(
            SemanticVersion.Parse("2.0.0"),
            SemanticVersion.Parse("2.0.0")
        );

        var response = await handler.HandleRequestAsync(request, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(MessageResponseCode.Success, response.ResponseCode);
        Assert.NotNull(response.ProtocolVersion);
    }

    [Fact]
    public async Task HandleRequestAsync_WithUnsupportedProtocolVersion_ReturnsError()
    {
        var handler = new HandshakeRequestHandler();
        var request = new HandshakeRequest(
            SemanticVersion.Parse("1.0.0"),
            SemanticVersion.Parse("1.0.0")
        );

        var response = await handler.HandleRequestAsync(request, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(MessageResponseCode.Error, response.ResponseCode);
    }
}
