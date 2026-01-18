using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.RequestHandlers
{
  /// <summary>
  /// Handles an <see cref="InitializeRequest"/>.
  /// </summary>
  internal class InitializeRequestHandler : RequestHandlerBase<InitializeRequest, InitializeResponse>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="InitializeRequestHandler"/> class.
    /// </summary>
    public InitializeRequestHandler()
    {
    }

    public override Task<InitializeResponse> HandleRequestAsync(InitializeRequest request)
    {
      Logger.Log(LogLevel.Verbose, $"Request timeout: {request.RequestTimeout}");
      return Task.FromResult(new InitializeResponse(MessageResponseCode.Success));
    }
  }
}
