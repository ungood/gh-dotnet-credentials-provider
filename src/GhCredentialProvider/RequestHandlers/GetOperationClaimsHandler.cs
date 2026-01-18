using GhCredentialProvider.GitHub;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.RequestHandlers;

internal class GetOperationClaimsHandler
    : BaseRequestHandler<GetOperationClaimsRequest, GetOperationClaimsResponse>
{
    public override Task<GetOperationClaimsResponse> HandleRequestAsync(
        GetOperationClaimsRequest request
    )
    {
        var operationClaims = new List<OperationClaim>();
        if (request.PackageSourceRepository == null || GitHubHostDetector.IsGitHubHost(request.PackageSourceRepository))
        {
            operationClaims.Add(OperationClaim.Authentication);
        }

        return Task.FromResult(new GetOperationClaimsResponse(operationClaims));
    }
}