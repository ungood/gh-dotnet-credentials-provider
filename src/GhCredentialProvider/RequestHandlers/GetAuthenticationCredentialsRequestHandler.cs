using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GhCredentialProvider.GitHub;
using NuGet.Common;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.RequestHandlers
{
  /// <summary>
  /// Handles a <see cref="GetAuthenticationCredentialsRequest"/> and replies with credentials.
  /// </summary>
  internal class GetAuthenticationCredentialsRequestHandler
    : RequestHandlerBase<GetAuthenticationCredentialsRequest, GetAuthenticationCredentialsResponse>
  {
    private const string FoundCredentialsForUri = "Found credentials for URI {0}";
    private const string CredentialsForUriNotFound = "Credentials for URI {0} not found";
    private readonly ITokenProvider _tokenProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAuthenticationCredentialsRequestHandler"/> class.
    /// </summary>
    /// <param name="tokenProvider">An <see cref="ITokenProvider"/> to use for retrieving tokens.</param>
    public GetAuthenticationCredentialsRequestHandler(ITokenProvider tokenProvider)
    {
      _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
    }

    public override async Task<GetAuthenticationCredentialsResponse> HandleRequestAsync(GetAuthenticationCredentialsRequest request)
    {
      try
      {
        var uriString = request.Uri?.ToString() ?? "";
        if (!GitHubHostDetector.IsGitHubHost(uriString))
        {
          Logger.Log(LogLevel.Verbose, string.Format(CredentialsForUriNotFound, request.Uri));
          return new GetAuthenticationCredentialsResponse(null, null, null, null, MessageResponseCode.NotFound);
        }

        var hostname = GitHubHostDetector.ExtractHostname(uriString) ?? "github.com";
        
        var token = await _tokenProvider.GetTokenAsync(hostname, default);

        if (string.IsNullOrWhiteSpace(token))
        {
          Logger.Log(LogLevel.Verbose, string.Format(CredentialsForUriNotFound, request.Uri));
          return new GetAuthenticationCredentialsResponse(null, null, "No GitHub token available", null, MessageResponseCode.NotFound);
        }

        Logger.Log(LogLevel.Verbose, string.Format(FoundCredentialsForUri, request.Uri));
        return new GetAuthenticationCredentialsResponse(
          "USERNAME", // GitHub Packages accepts any username when using PAT
          token,
          null,
          new List<string> {"basic"},
          MessageResponseCode.Success);
      }
      catch (Exception e)
      {
        Logger.Log(LogLevel.Error, $"Failed to acquire credentials: {e}");

        return new GetAuthenticationCredentialsResponse(
          username: null,
          password: null,
          message: e.Message,
          authenticationTypes: null,
          responseCode: MessageResponseCode.Error);
      }
    }
  }
}
