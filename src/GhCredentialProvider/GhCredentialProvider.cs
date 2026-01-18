using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GhCredentialProvider.GitHub;
using GhCredentialProvider.Logging;
using NuGet.Common;
using NuGet.Protocol.Plugins;
using ILogger = GhCredentialProvider.Logging.ILogger;

namespace GhCredentialProvider
{
  internal class GhCredentialProvider : ICredentialProvider
  {
    private const string CouldProvideCredentialsForUri = "Could provide credentials for URI {0}";
    private const string CouldNotProvideCredentialsForUri = "Could not provide credentials for URI {0}";
    private const string FoundCredentialsForUri = "Found credentials for URI {0}";
    private const string CredentialsForUriNotFound = "Credentials for URI {0} not found";
    private readonly ILogger _logger;
    private readonly ITokenProvider _tokenProvider;

    internal GhCredentialProvider(ILogger logger, ITokenProvider tokenProvider)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
    }

    public bool CanProvideCredentials(Uri uri)
    {
      var uriString = uri?.ToString() ?? "";
      if (GitHubHostDetector.IsGitHubHost(uriString))
      {
        _logger.Log(LogLevel.Verbose, string.Format(CouldProvideCredentialsForUri, uri));
        return true;
      }

      _logger.Log(LogLevel.Verbose, string.Format(CouldNotProvideCredentialsForUri, uri));
      return false;
    }

    public GetAuthenticationCredentialsResponse HandleRequest(GetAuthenticationCredentialsRequest request)
    {
      var uriString = request.Uri?.ToString() ?? "";
      if (!GitHubHostDetector.IsGitHubHost(uriString))
      {
        _logger.Log(LogLevel.Verbose, string.Format(CredentialsForUriNotFound, request.Uri));
        return new GetAuthenticationCredentialsResponse(null, null, null, null, MessageResponseCode.NotFound);
      }

      var hostname = GitHubHostDetector.ExtractHostname(uriString) ?? "github.com";
      
      // Get token synchronously - we'll need to block on async call
      var tokenTask = _tokenProvider.GetTokenAsync(hostname, default);
      var token = tokenTask.GetAwaiter().GetResult();

      if (string.IsNullOrWhiteSpace(token))
      {
        _logger.Log(LogLevel.Verbose, string.Format(CredentialsForUriNotFound, request.Uri));
        return new GetAuthenticationCredentialsResponse(null, null, "No GitHub token available", null, MessageResponseCode.NotFound);
      }

      _logger.Log(LogLevel.Verbose, string.Format(FoundCredentialsForUri, request.Uri));
      return new GetAuthenticationCredentialsResponse(
        "USERNAME", // GitHub Packages accepts any username when using PAT
        token,
        null,
        new List<string> {"basic"},
        MessageResponseCode.Success);
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
    }
  }
}
