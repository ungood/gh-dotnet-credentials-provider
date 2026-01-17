using GhCredentialProvider.Handlers;
using Newtonsoft.Json.Linq;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.Plugin;

public class PluginMessageDispatcher
{
    private readonly Dictionary<Type, IMessageHandler> _handlers;
    private static readonly Dictionary<MessageMethod, Type> MessageMethodToRequestType = new()
    {
        { MessageMethod.Handshake, typeof(HandshakeRequest) },
        { MessageMethod.Initialize, typeof(InitializeRequest) },
        { MessageMethod.GetOperationClaims, typeof(GetOperationClaimsRequest) },
        { MessageMethod.GetAuthenticationCredentials, typeof(GetAuthenticationCredentialsRequest) },
        { MessageMethod.SetLogLevel, typeof(SetLogLevelRequest) }
    };

    public PluginMessageDispatcher(Dictionary<Type, IMessageHandler> handlers)
    {
        _handlers = handlers;
    }

    public async Task<Message?> DispatchAsync(Message request, CancellationToken cancellationToken = default)
    {
        // Get the request type from the message method
        if (!MessageMethodToRequestType.TryGetValue(request.Method, out var requestType))
        {
            // Return unsupported response
            var errorResponse = new InitializeResponse(MessageResponseCode.Error);
            var payloadJson = JObject.FromObject(errorResponse);
            return new Message(request.RequestId, MessageType.Response, request.Method, payloadJson);
        }

        // Route by request type
        if (_handlers.TryGetValue(requestType, out var handler))
        {
            return await handler.HandleAsync(request, cancellationToken);
        }

        // Return unsupported response
        var unsupportedResponse = new InitializeResponse(MessageResponseCode.Error);
        var unsupportedPayloadJson = JObject.FromObject(unsupportedResponse);
        return new Message(request.RequestId, MessageType.Response, request.Method, unsupportedPayloadJson);
    }
}
