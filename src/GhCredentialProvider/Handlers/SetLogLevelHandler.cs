using GhCredentialProvider.Logging;
using NuGet.Common;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.Handlers;

public class SetLogLevelRequestHandler : IRequestHandler
{
    private readonly ILogger _logger;

    public SetLogLevelRequestHandler()
    {
        _logger = new StandardErrorLogger(nameof(SetLogLevelRequestHandler));
    }

    public Task<SetLogLevelResponse> HandleRequestAsync(
        SetLogLevelRequest request,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation($"Received SetLogLevel request: LogLevel={request.LogLevel}");

        // TODO: Update logger verbosity level based on request
        var response = new SetLogLevelResponse(MessageResponseCode.Success);

        _logger.LogInformation(
            $"Sending SetLogLevel response: ResponseCode={response.ResponseCode}"
        );
        return Task.FromResult(response);
    }

    public Task HandleResponseAsync(
        IConnection connection,
        Message message,
        IResponseHandler responseHandler,
        CancellationToken cancellationToken
    )
    {
        // This method is for handling responses, not requests
        // For request handlers, this is typically not used
        return Task.CompletedTask;
    }

    public CancellationToken CancellationToken => CancellationToken.None;
}
