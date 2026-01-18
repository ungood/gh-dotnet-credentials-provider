using System.Threading.Tasks;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.RequestHandlers
{
    /// <summary>
    /// Handles a <see cref="SetLogLevelRequest"/> and replies with credentials.
    /// </summary>
    internal class SetLogLevelHandler : RequestHandlerBase<SetLogLevelRequest, SetLogLevelResponse>
    {
        public SetLogLevelHandler() { }

        public override Task<SetLogLevelResponse> HandleRequestAsync(SetLogLevelRequest request)
        {
            Logger.SetLogLevel(request.LogLevel);
            return Task.FromResult(new SetLogLevelResponse(MessageResponseCode.Success));
        }
    }
}
