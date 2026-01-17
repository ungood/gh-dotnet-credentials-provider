using GhCredentialProvider.GitHub;
using Newtonsoft.Json.Linq;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.Handlers;

public class GetAuthenticationCredentialsHandler : IMessageHandler
{
    private readonly ITokenProvider _tokenProvider;

    public GetAuthenticationCredentialsHandler(ITokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    public async Task<Message> HandleAsync(Message request, CancellationToken cancellationToken = default)
    {
        var payload = MessageUtilities.DeserializePayload<GetAuthenticationCredentialsRequest>(request);
        if (payload == null)
        {
            return CreateErrorResponse(request.RequestId, "Invalid request");
        }

        // Verify this is a GitHub host
        var uri = payload.Uri?.ToString() ?? "";
        if (!GitHubHostDetector.IsGitHubHost(uri))
        {
            return CreateErrorResponse(request.RequestId, "Not a GitHub package source");
        }

        // Extract hostname for token retrieval
        var hostname = GitHubHostDetector.ExtractHostname(uri) ?? "github.com";

        // Get token
        var token = await _tokenProvider.GetTokenAsync(hostname, cancellationToken);

        if (string.IsNullOrWhiteSpace(token))
        {
            if (payload.IsNonInteractive)
            {
                return CreateErrorResponse(request.RequestId, 
                    "No GitHub token available and non-interactive mode is enabled. Set GH_TOKEN or GITHUB_TOKEN environment variable, or run 'gh auth login'.");
            }

            return CreateErrorResponse(request.RequestId, 
                "Unable to retrieve GitHub token. Ensure 'gh' CLI is installed and authenticated, or set GH_TOKEN or GITHUB_TOKEN environment variable.");
        }

        // Return credentials
        var response = new GetAuthenticationCredentialsResponse(
            username: "USERNAME", // GitHub Packages accepts any username when using PAT
            password: token,
            message: "Credentials retrieved successfully",
            authenticationTypes: new List<string> { "GitHubPAT" },
            responseCode: MessageResponseCode.Success
        );
        var payloadJson = JObject.FromObject(response);
        return new Message(request.RequestId, MessageType.Response, MessageMethod.GetAuthenticationCredentials, payloadJson);
    }

    private static Message CreateErrorResponse(string requestId, string message)
    {
        var errorResponse = new GetAuthenticationCredentialsResponse(
            username: null,
            password: null,
            message: message,
            authenticationTypes: new List<string>(),
            responseCode: MessageResponseCode.Error
        );
        var payloadJson = JObject.FromObject(errorResponse);
        return new Message(requestId, MessageType.Response, MessageMethod.GetAuthenticationCredentials, payloadJson);
    }
}
