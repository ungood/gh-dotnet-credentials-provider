using System.Threading.Tasks;
using GhCredentialProvider.Logging;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.RequestHandlers
{
  /// <summary>
  /// Handles a <see cref="SetLogLevelRequest"/> and replies with credentials.
  /// </summary>
  internal class SetLogLevelHandler : RequestHandlerBase<SetLogLevelRequest, SetLogLevelResponse>
  {
    public SetLogLevelHandler(ILogger logger) : base(logger)
    {
    }

    public override Task<SetLogLevelResponse> HandleRequestAsync(SetLogLevelRequest request)
    {
      Logger.SetLogLevel(request.LogLevel);
      return Task.FromResult(new SetLogLevelResponse(MessageResponseCode.Success));
    }
  }
}
