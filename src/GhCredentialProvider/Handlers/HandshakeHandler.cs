using NuGet.Protocol.Plugins;
using NuGet.Versioning;

namespace GhCredentialProvider.Handlers;

public class HandshakeRequestHandler : IRequestHandler
{
    private const string SupportedProtocolVersion = "2.0.0";
    private const string MinimumProtocolVersion = "2.0.0";

    public Task<HandshakeResponse> HandleRequestAsync(
        HandshakeRequest request,
        CancellationToken cancellationToken
    )
    {
        // Negotiate protocol version
        var requestedVersion = request.ProtocolVersion;
        var minimumVersion = request.MinimumProtocolVersion;

        // Check if we support the requested version
        var supported = SemanticVersion.Parse(SupportedProtocolVersion);
        var minimum = SemanticVersion.Parse(MinimumProtocolVersion);

        if (requestedVersion >= minimum && requestedVersion <= supported)
        {
            var response = new HandshakeResponse(MessageResponseCode.Success, supported);
            return Task.FromResult(response);
        }

        // Version negotiation failed
        var errorResponse = new HandshakeResponse(MessageResponseCode.Error, null);
        return Task.FromResult(errorResponse);
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
