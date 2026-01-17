using GhCredentialProvider.GitHub;
using Newtonsoft.Json.Linq;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.Handlers;

public class GetOperationClaimsHandler : IMessageHandler
{
    public Task<Message> HandleAsync(Message request, CancellationToken cancellationToken = default)
    {
        var payload = MessageUtilities.DeserializePayload<GetOperationClaimsRequest>(request);
        if (payload == null)
        {
            return Task.FromResult(CreateErrorResponse(request.RequestId));
        }

        // Check if the package source is a GitHub host
        var packageSource = payload.PackageSourceRepository?.ToString() ?? "";
        var serviceIndex = payload.ServiceIndex?.ToString() ?? "";
        var isGitHub =
            GitHubHostDetector.IsGitHubHost(packageSource)
            || GitHubHostDetector.IsGitHubHost(serviceIndex);

        var claims = new List<OperationClaim>();
        if (isGitHub)
        {
            claims.Add(OperationClaim.Authentication);
        }

        var response = new GetOperationClaimsResponse(claims);
        var payloadJson = JObject.FromObject(response);
        return Task.FromResult(
            new Message(
                request.RequestId,
                MessageType.Response,
                MessageMethod.GetOperationClaims,
                payloadJson
            )
        );
    }

    private static Message CreateErrorResponse(string requestId)
    {
        var errorResponse = new GetOperationClaimsResponse(new List<OperationClaim>());
        var payloadJson = JObject.FromObject(errorResponse);
        return new Message(
            requestId,
            MessageType.Response,
            MessageMethod.GetOperationClaims,
            payloadJson
        );
    }
}
