using Newtonsoft.Json.Linq;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.Handlers;

public class InitializeHandler : IMessageHandler
{
    public Task<Message> HandleAsync(Message request, CancellationToken cancellationToken = default)
    {
        var payload = MessageUtilities.DeserializePayload<InitializeRequest>(request);
        if (payload == null)
        {
            return Task.FromResult(CreateErrorResponse(request.RequestId));
        }

        // Store client version and settings if needed
        // For now, just acknowledge initialization

        var response = new InitializeResponse(MessageResponseCode.Success);
        var payloadJson = JObject.FromObject(response);
        return Task.FromResult(
            new Message(
                request.RequestId,
                MessageType.Response,
                MessageMethod.Initialize,
                payloadJson
            )
        );
    }

    private static Message CreateErrorResponse(string requestId)
    {
        var errorResponse = new InitializeResponse(MessageResponseCode.Error);
        var payloadJson = JObject.FromObject(errorResponse);
        return new Message(requestId, MessageType.Response, MessageMethod.Initialize, payloadJson);
    }
}
