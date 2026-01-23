using NuGet.Protocol.Plugins;

namespace GithubCredentialProvider.RequestHandlers;

internal class SetLogLevelHandler : BaseRequestHandler<SetLogLevelRequest, SetLogLevelResponse>
{
    public override Task<SetLogLevelResponse> HandleRequestAsync(SetLogLevelRequest request)
    {
        Logger.SetLogLevel(request.LogLevel);
        return Task.FromResult(new SetLogLevelResponse(MessageResponseCode.Success));
    }
}