using Newtonsoft.Json.Linq;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.Handlers;

public class SetLogLevelHandler : IMessageHandler
{
    public Task<Message> HandleAsync(Message request, CancellationToken cancellationToken = default)
    {
        var payload = MessageUtilities.DeserializePayload<SetLogLevelRequest>(request);
        if (payload == null)
        {
            return Task.FromResult(CreateErrorResponse(request.RequestId));
        }

        // Store log level if needed for future logging
        // For now, just acknowledge

        var response = new SetLogLevelResponse(MessageResponseCode.Success);
        var payloadJson = JObject.FromObject(response);
        return Task.FromResult(
            new Message(
                request.RequestId,
                MessageType.Response,
                MessageMethod.SetLogLevel,
                payloadJson
            )
        );
    }

    private static Message CreateErrorResponse(string requestId)
    {
        var errorResponse = new SetLogLevelResponse(MessageResponseCode.Error);
        var payloadJson = JObject.FromObject(errorResponse);
        return new Message(requestId, MessageType.Response, MessageMethod.SetLogLevel, payloadJson);
    }
}
