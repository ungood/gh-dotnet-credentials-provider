using NuGet.Common;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.RequestHandlers;

/// <summary>
///     Handles an <see cref="InitializeRequest" />.
/// </summary>
internal class InitializeHandler
    : BaseRequestHandler<InitializeRequest, InitializeResponse>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InitializeHandler" /> class.
    /// </summary>
    public InitializeHandler()
    {
    }

    public override Task<InitializeResponse> HandleRequestAsync(InitializeRequest request)
    {
        Logger.Log(LogLevel.Verbose, $"Request timeout: {request.RequestTimeout}");
        return Task.FromResult(new InitializeResponse(MessageResponseCode.Success));
    }
}