using Newtonsoft.Json.Linq;
using NuGet.Common;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.Handlers;

public class SetLogLevelHandler : IMessageHandler
{
    private readonly ILogger _logger;

    public SetLogLevelHandler(ILogger logger)
    {
        _logger = logger;
    }

    public Task<Message> HandleAsync(Message request, CancellationToken cancellationToken = default)
    {
        var payload = MessageUtilities.DeserializePayload<SetLogLevelRequest>(request);
        if (payload == null)
        {
            return Task.FromResult(CreateErrorResponse(request.RequestId));
        }

        // Note: NuGet.Common.ILogger doesn't have a SetLogLevel method
        // The log level is typically controlled by the logger implementation itself
        _logger.LogInformation($"Log level requested: {payload.LogLevel}");

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

    private Message CreateErrorResponse(string requestId)
    {
        _logger.LogWarning($"Failed to set log level for request {requestId}");
        var errorResponse = new SetLogLevelResponse(MessageResponseCode.Error);
        var payloadJson = JObject.FromObject(errorResponse);
        return new Message(requestId, MessageType.Response, MessageMethod.SetLogLevel, payloadJson);
    }
}
