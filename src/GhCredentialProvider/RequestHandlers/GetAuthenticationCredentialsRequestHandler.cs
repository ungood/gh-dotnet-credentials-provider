using GhCredentialProvider.GitHub;
using NuGet.Common;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.RequestHandlers;

/// <summary>
///     Handles a <see cref="GetAuthenticationCredentialsRequest" /> and replies with credentials.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="GetAuthenticationCredentialsRequestHandler" /> class.
/// </remarks>
/// <param name="tokenProvider">An <see cref="ITokenProvider" /> to use for retrieving tokens.</param>
internal class GetAuthenticationCredentialsRequestHandler(ITokenProvider tokenProvider)
    : BaseRequestHandler<
        GetAuthenticationCredentialsRequest,
        GetAuthenticationCredentialsResponse
    >
{
    private readonly ITokenProvider _tokenProvider =
        tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));

    public override async Task<GetAuthenticationCredentialsResponse> HandleRequestAsync(
        GetAuthenticationCredentialsRequest request
    )
    {
        try
        {
            var uriString = request.Uri?.ToString() ?? "";
            if (!GitHubHostDetector.IsGitHubHost(uriString))
                return CreateErrorResponse(MessageResponseCode.NotFound,
                    $"Credentials for URI {request.Uri} not found");

            var hostname = GitHubHostDetector.ExtractHostname(uriString) ?? "github.com";
            var token = await _tokenProvider.GetTokenAsync(hostname);

            if (string.IsNullOrWhiteSpace(token))
                return CreateErrorResponse(MessageResponseCode.NotFound,
                    $"Credentials for URI {request.Uri} not found");

            Logger.Log(LogLevel.Verbose, $"Found credentials for URI {request.Uri}");
            return new GetAuthenticationCredentialsResponse(
                "USERNAME", // GitHub Packages accepts any username when using PAT
                token,
                null,
                ["basic"],
                MessageResponseCode.Success
            );
        }
        catch (Exception e)
        {
            return CreateErrorResponse(MessageResponseCode.Error, $"Failed to acquire credentials: {e}");
        }
    }

    private GetAuthenticationCredentialsResponse CreateErrorResponse(
        MessageResponseCode responseCode,
        string message
    )
    {
        Logger.Log(LogLevel.Error, message);

        return new GetAuthenticationCredentialsResponse(
            null,
            null,
            message,
            null,
            responseCode
        );
    }
}