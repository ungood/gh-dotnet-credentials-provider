using GhCredentialProvider.Handlers;
using Newtonsoft.Json.Linq;
using NuGet.Protocol.Plugins;
using NuGet.Versioning;
using Xunit;

namespace GhCredentialProvider.Tests.Handlers;

public class HandshakeHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidProtocolVersion_ReturnsSuccess()
    {
        var handler = new HandshakeHandler();
        var requestPayload = new HandshakeRequest(
            SemanticVersion.Parse("2.0.0"),
            SemanticVersion.Parse("2.0.0")
        );
        var payloadJson = JObject.FromObject(requestPayload);
        var request = new Message(
            "test-123",
            MessageType.Request,
            MessageMethod.Handshake,
            payloadJson
        );

        var response = await handler.HandleAsync(request);

        Assert.NotNull(response);
        Assert.Equal(MessageType.Response, response.Type);
        Assert.Equal("test-123", response.RequestId);
        var responsePayload = MessageUtilities.DeserializePayload<HandshakeResponse>(response);
        Assert.NotNull(responsePayload);
        Assert.Equal(MessageResponseCode.Success, responsePayload.ResponseCode);
        Assert.NotNull(responsePayload.ProtocolVersion);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidPayload_ReturnsError()
    {
        var handler = new HandshakeHandler();
        // Create a message with invalid payload (null payload)
        var request = new Message("test-123", MessageType.Request, MessageMethod.Handshake, null);

        var response = await handler.HandleAsync(request);

        Assert.NotNull(response);
        Assert.Equal(MessageType.Response, response.Type);
        var responsePayload = MessageUtilities.DeserializePayload<HandshakeResponse>(response);
        Assert.NotNull(responsePayload);
        Assert.Equal(MessageResponseCode.Error, responsePayload.ResponseCode);
    }

    [Fact]
    public async Task HandleAsync_WithUnsupportedProtocolVersion_ReturnsError()
    {
        var handler = new HandshakeHandler();
        var requestPayload = new HandshakeRequest(
            SemanticVersion.Parse("1.0.0"),
            SemanticVersion.Parse("1.0.0")
        );
        var payloadJson = JObject.FromObject(requestPayload);
        var request = new Message(
            "test-123",
            MessageType.Request,
            MessageMethod.Handshake,
            payloadJson
        );

        var response = await handler.HandleAsync(request);

        Assert.NotNull(response);
        Assert.Equal(MessageType.Response, response.Type);
        var responsePayload = MessageUtilities.DeserializePayload<HandshakeResponse>(response);
        Assert.NotNull(responsePayload);
        Assert.Equal(MessageResponseCode.Error, responsePayload.ResponseCode);
    }
}
