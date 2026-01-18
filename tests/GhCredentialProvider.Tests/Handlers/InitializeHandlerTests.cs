using GhCredentialProvider.RequestHandlers;
using NuGet.Protocol.Plugins;
using Xunit;

namespace GhCredentialProvider.Tests.Handlers;

public class InitializeHandlerTests
{
    [Fact]
    public async Task HandleRequestAsync_WithValidRequest_ReturnsSuccess()
    {
        var handler = new InitializeRequestHandler();
        var request = new InitializeRequest("6.0.0", "en-US", TimeSpan.FromSeconds(5));

        var response = await handler.HandleRequestAsync(request);

        Assert.NotNull(response);
        Assert.Equal(MessageResponseCode.Success, response.ResponseCode);
    }
}
