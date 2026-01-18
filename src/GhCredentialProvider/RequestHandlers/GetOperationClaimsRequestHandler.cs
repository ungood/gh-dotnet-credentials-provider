using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GhCredentialProvider.GitHub;
using NuGet.Common;
using NuGet.Protocol.Plugins;
using NuGet.Versioning;

namespace GhCredentialProvider.RequestHandlers
{
    /// <summary>
    /// Handles a <see cref="GetOperationClaimsRequest"/> and replies with the supported operations.
    /// </summary>
    internal class GetOperationClaimsRequestHandler
        : BaseRequestHandler<GetOperationClaimsRequest, GetOperationClaimsResponse>
    {
        private readonly bool mySupportAuthentication;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetOperationClaimsRequestHandler"/> class.
        /// </summary>
        /// <param name="sdkInfo">Sdk info provider.</param>
        public GetOperationClaimsRequestHandler(SdkInfo sdkInfo)
        {
            var hasVersion = sdkInfo.TryGetSdkVersion(out var semanticVersion);
            Logger.Log(
                LogLevel.Verbose,
                hasVersion
                    ? $".NET SDK {semanticVersion} was detected."
                    : ".NET SDK was not detected."
            );

            mySupportAuthentication =
                !hasVersion || semanticVersion >= new SemanticVersion(2, 1, 400);
            Logger.Log(
                LogLevel.Verbose,
                mySupportAuthentication
                    ? "Authentication is supported."
                    : "Authentication not is supported."
            );
        }

        public override Task<GetOperationClaimsResponse> HandleRequestAsync(
            GetOperationClaimsRequest request
        )
        {
            var operationClaims = new List<OperationClaim>();
            try
            {
                if (mySupportAuthentication)
                {
                    if (
                        (request.PackageSourceRepository == null && request.ServiceIndex == null)
                        || (
                            Uri.TryCreate(
                                request.PackageSourceRepository,
                                UriKind.Absolute,
                                out Uri uri
                            ) && GitHubHostDetector.IsGitHubHost(uri.ToString())
                        )
                    )
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
