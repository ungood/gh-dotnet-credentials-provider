using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GhCredentialProvider.GitHub;
using NuGet.Common;
using NuGet.Protocol.Plugins;
using NuGet.Versioning;
using ILogger = GhCredentialProvider.Logging.ILogger;

namespace GhCredentialProvider.RequestHandlers
{
  /// <summary>
  /// Handles a <see cref="GetOperationClaimsRequest"/> and replies with the supported operations.
  /// </summary>
  internal class GetOperationClaimsRequestHandler : RequestHandlerBase<GetOperationClaimsRequest, GetOperationClaimsResponse>
  {
    private readonly bool mySupportAuthentication;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetOperationClaimsRequestHandler"/> class.
    /// </summary>
    /// <param name="logger">A <see cref="Logging.ILogger"/> to use for logging.</param>
    /// <param name="sdkInfo">Sdk info provider.</param>
    public GetOperationClaimsRequestHandler(ILogger logger, SdkInfo sdkInfo) : base(logger)
    {
      var hasVersion = sdkInfo.TryGetSdkVersion(out var semanticVersion);
      logger.Log(LogLevel.Verbose, hasVersion ? $".NET SDK {semanticVersion} was detected." : ".NET SDK was not detected.");

      mySupportAuthentication = !hasVersion || semanticVersion >= new SemanticVersion(2, 1, 400);
      logger.Log(LogLevel.Verbose, mySupportAuthentication ? "Authentication is supported." : "Authentication not is supported.");
    }

    public override Task<GetOperationClaimsResponse> HandleRequestAsync(GetOperationClaimsRequest request)
    {
      var operationClaims = new List<OperationClaim>();
      try
      {
        if (mySupportAuthentication)
        {
          if ((request.PackageSourceRepository == null && request.ServiceIndex == null) ||
              (Uri.TryCreate(request.PackageSourceRepository, UriKind.Absolute, out Uri uri) &&
              GitHubHostDetector.IsGitHubHost(uri.ToString())))
          {
            operationClaims.Add(OperationClaim.Authentication);            
          }
        }
      }
      catch (Exception e)
      {
        Logger.Log(LogLevel.Error, $"Failed to execute credentials provider: {e}");
      }

      return Task.FromResult(new GetOperationClaimsResponse(operationClaims));
    }
  }
}
