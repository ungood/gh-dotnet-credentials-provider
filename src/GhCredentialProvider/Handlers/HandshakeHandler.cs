using Newtonsoft.Json.Linq;
using NuGet.Protocol.Plugins;
using NuGet.Versioning;

namespace GhCredentialProvider.Handlers;

public class HandshakeHandler : IMessageHandler
{
    private const string SupportedProtocolVersion = "2.0.0";
    private const string MinimumProtocolVersion = "2.0.0";

    public Task<Message> HandleAsync(Message request, CancellationToken cancellationToken = default)
    {
        var payload = MessageUtilities.DeserializePayload<HandshakeRequest>(request);
        if (payload == null)
        {
            return Task.FromResult(CreateErrorResponse(request.RequestId));
        }

        // Negotiate protocol version
        var requestedVersion = payload.ProtocolVersion;
        var minimumVersion = payload.MinimumProtocolVersion;

        // Check if we support the requested version
        var supported = SemanticVersion.Parse(SupportedProtocolVersion);
        var minimum = SemanticVersion.Parse(MinimumProtocolVersion);

        if (requestedVersion >= minimum && requestedVersion <= supported)
        {
            var response = new HandshakeResponse(MessageResponseCode.Success, supported);
            var payloadJson = JObject.FromObject(response);
            return Task.FromResult(
                new Message(
                    request.RequestId,
                    MessageType.Response,
                    MessageMethod.Handshake,
                    payloadJson
                )
            );
        }

        // Version negotiation failed
        return Task.FromResult(CreateErrorResponse(request.RequestId));
    }

    private static Message CreateErrorResponse(string requestId)
    {
        var errorResponse = new HandshakeResponse(MessageResponseCode.Error, null);
        var payloadJson = JObject.FromObject(errorResponse);
        return new Message(requestId, MessageType.Response, MessageMethod.Handshake, payloadJson);
    }
}
