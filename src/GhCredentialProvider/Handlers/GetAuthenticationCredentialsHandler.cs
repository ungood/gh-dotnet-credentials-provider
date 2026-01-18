using GhCredentialProvider.GitHub;
using GhCredentialProvider.Logging;
using NuGet.Common;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.Handlers;

public class GetAuthenticationCredentialsRequestHandler : IRequestHandler
{
    private readonly ILogger _logger = new StandardErrorLogger(
        nameof(GetAuthenticationCredentialsRequestHandler)
    );
    private readonly ITokenProvider _tokenProvider;

    public GetAuthenticationCredentialsRequestHandler(ITokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    public async Task<GetAuthenticationCredentialsResponse> HandleRequestAsync(
        GetAuthenticationCredentialsRequest request,
        CancellationToken cancellationToken
    )
    {
        var uri = request.Uri?.ToString() ?? "";
        _logger.LogInformation(
            $"Received GetAuthenticationCredentials request: Uri={uri}, IsNonInteractive={request.IsNonInteractive}"
        );

        // Verify this is a GitHub host
        if (!GitHubHostDetector.IsGitHubHost(uri))
        {
            _logger.LogWarning($"Not a GitHub host: {uri}");
            var response = new GetAuthenticationCredentialsResponse(
                username: null,
                password: null,
                message: "Not a GitHub package source",
                authenticationTypes: new List<string>(),
                responseCode: MessageResponseCode.Error
            );
            _logger.LogInformation(
                $"Sending GetAuthenticationCredentials response: ResponseCode={response.ResponseCode}, Message={response.Message}"
            );
            return response;
        }

        var hostname = GitHubHostDetector.ExtractHostname(uri) ?? "github.com";
        _logger.LogInformation($"Extracted hostname: {hostname}");

        _logger.LogDebug($"Attempting to retrieve token for hostname: {hostname}");
        var token = await _tokenProvider.GetTokenAsync(hostname, cancellationToken);

        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning($"No token retrieved for hostname: {hostname}");
            GetAuthenticationCredentialsResponse response;

            if (request.IsNonInteractive)
            {
                response = new GetAuthenticationCredentialsResponse(
                    username: null,
                    password: null,
                    message: "No GitHub token available and non-interactive mode is enabled. Set GH_TOKEN or GITHUB_TOKEN environment variable, or run 'gh auth login'.",
                    authenticationTypes: new List<string>(),
                    responseCode: MessageResponseCode.Error
                );
            }
            else
            {
                response = new GetAuthenticationCredentialsResponse(
                    username: null,
                    password: null,
                    message: "Unable to retrieve GitHub token. Ensure 'gh' CLI is installed and authenticated, or set GH_TOKEN or GITHUB_TOKEN environment variable.",
                    authenticationTypes: new List<string>(),
                    responseCode: MessageResponseCode.Error
                );
            }

            _logger.LogInformation(
                $"Sending GetAuthenticationCredentials response: ResponseCode={response.ResponseCode}, Message={response.Message}"
            );
            return response;
        }

        // Return credentials
        _logger.LogInformation($"Token retrieved successfully for hostname: {hostname}");
        var successResponse = new GetAuthenticationCredentialsResponse(
            username: "USERNAME", // GitHub Packages accepts any username when using PAT
            password: token,
            message: "Credentials retrieved successfully",
            authenticationTypes: new List<string> { "GitHubPAT" },
            responseCode: MessageResponseCode.Success
        );
        _logger.LogInformation(
            $"Sending GetAuthenticationCredentials response: ResponseCode={successResponse.ResponseCode}, Message={successResponse.Message}"
        );
        return successResponse;
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

    public CancellationToken CancellationToken { get; }
}
