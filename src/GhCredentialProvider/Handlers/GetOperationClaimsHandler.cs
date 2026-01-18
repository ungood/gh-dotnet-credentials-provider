using GhCredentialProvider.GitHub;
using GhCredentialProvider.Logging;
using NuGet.Common;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.Handlers;

public class GetOperationClaimsRequestHandler : IRequestHandler
{
    private readonly ILogger _logger;

    public GetOperationClaimsRequestHandler()
    {
        _logger = new StandardErrorLogger(nameof(GetOperationClaimsRequestHandler));
    }

    public Task<GetOperationClaimsResponse> HandleRequestAsync(
        GetOperationClaimsRequest request,
        CancellationToken cancellationToken
    )
    {
        var packageSource = request.PackageSourceRepository?.ToString() ?? "";
        var serviceIndex = request.ServiceIndex?.ToString() ?? "";
        _logger.LogInformation(
            $"Received GetOperationClaims request: PackageSource={packageSource}, ServiceIndex={serviceIndex}"
        );

        // Check if the package source is a GitHub host
        var isGitHub =
            GitHubHostDetector.IsGitHubHost(packageSource)
            || GitHubHostDetector.IsGitHubHost(serviceIndex);

        var claims = new List<OperationClaim>();
        if (isGitHub)
        {
            claims.Add(OperationClaim.Authentication);
            _logger.LogInformation($"GitHub host detected, adding Authentication claim");
        }
        else
        {
            _logger.LogInformation($"Not a GitHub host, no claims added");
        }

        var response = new GetOperationClaimsResponse(claims);
        _logger.LogInformation(
            $"Sending GetOperationClaims response: Claims={string.Join(", ", claims)}"
        );
        return Task.FromResult(response);
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
