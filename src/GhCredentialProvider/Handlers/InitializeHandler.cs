using GhCredentialProvider.Logging;
using NuGet.Common;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.Handlers;

public class InitializeRequestHandler : IRequestHandler
{
    private readonly ILogger _logger;

    public InitializeRequestHandler()
    {
        _logger = new StandardErrorLogger(nameof(InitializeRequestHandler));
    }

    public Task<InitializeResponse> HandleRequestAsync(
        InitializeRequest request,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation(
            $"Received Initialize request: ClientVersion={request.ClientVersion}"
        );

        // Store client version and settings if needed
        // For now, just acknowledge initialization
        var response = new InitializeResponse(MessageResponseCode.Success);

        _logger.LogInformation(
            $"Sending Initialize response: ResponseCode={response.ResponseCode}"
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
