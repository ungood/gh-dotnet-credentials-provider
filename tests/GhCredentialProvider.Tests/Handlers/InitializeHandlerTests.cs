using GhCredentialProvider.Handlers;
using Newtonsoft.Json.Linq;
using NuGet.Protocol.Plugins;
using Xunit;

namespace GhCredentialProvider.Tests.Handlers;

public class InitializeHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidRequest_ReturnsSuccess()
    {
        var handler = new InitializeHandler();
        var requestPayload = new InitializeRequest("6.0.0", "en-US", TimeSpan.FromSeconds(5));
        var payloadJson = JObject.FromObject(requestPayload);
        var request = new Message(
            "init-456",
            MessageType.Request,
            MessageMethod.Initialize,
            payloadJson
        );

        var response = await handler.HandleAsync(request);

        Assert.NotNull(response);
        Assert.Equal(MessageType.Response, response.Type);
        Assert.Equal("init-456", response.RequestId);
        var responsePayload = MessageUtilities.DeserializePayload<InitializeResponse>(response);
        Assert.NotNull(responsePayload);
        Assert.Equal(MessageResponseCode.Success, responsePayload.ResponseCode);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidPayload_ReturnsError()
    {
        var handler = new InitializeHandler();
        // Create a message with invalid payload (null payload)
        var request = new Message("test-123", MessageType.Request, MessageMethod.Initialize, null);

        var response = await handler.HandleAsync(request);

        Assert.NotNull(response);
        Assert.Equal(MessageType.Response, response.Type);
        var responsePayload = MessageUtilities.DeserializePayload<InitializeResponse>(response);
        Assert.NotNull(responsePayload);
        Assert.Equal(MessageResponseCode.Error, responsePayload.ResponseCode);
    }
}
