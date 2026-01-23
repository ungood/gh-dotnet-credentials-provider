using NuGet.Common;
using NuGet.Protocol.Plugins;

namespace GithubCredentialProvider.RequestHandlers;

internal class InitializeHandler
    : BaseRequestHandler<InitializeRequest, InitializeResponse>
{
    public InitializeHandler()
    {
    }

    public override Task<InitializeResponse> HandleRequestAsync(InitializeRequest request)
    {
        Logger.Log(LogLevel.Verbose, $"Request timeout: {request.RequestTimeout}");
        return Task.FromResult(new InitializeResponse(MessageResponseCode.Success));
    }
}