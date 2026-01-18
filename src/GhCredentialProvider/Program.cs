using GhCredentialProvider.GitHub;
using GhCredentialProvider.Handlers;
using GhCredentialProvider.Logging;
using NuGet.Common;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider;

class Program
{
    private static readonly ILogger _logger = new StandardErrorLogger(
        Environment.ProcessPath ?? "gh-dotnet-credential-provider"
    );

    static async Task<int> Main(string[] args)
    {
        // Check if running as a plugin
        if (args.Length == 0 || args[0] != "-Plugin")
        {
            Console.WriteLine("GitHub NuGet Credential Provider");
            Console.WriteLine(
                "This is a NuGet cross-platform plugin. It should be invoked by NuGet client tools."
            );
            Console.WriteLine(
                "For more information, see: https://learn.microsoft.com/en-us/nuget/reference/extensibility/nuget-cross-platform-plugins"
            );
            return 1;
        }

        _logger.LogInformation("Starting GitHub NuGet Credential Provider plugin");

        try
        {
            var requestHandlers = new RequestHandlers();
            requestHandlers.TryAdd(MessageMethod.Initialize, new InitializeRequestHandler());
            requestHandlers.TryAdd(
                MessageMethod.GetOperationClaims,
                new GetOperationClaimsRequestHandler()
            );
            requestHandlers.TryAdd(
                MessageMethod.GetAuthenticationCredentials,
                new GetAuthenticationCredentialsRequestHandler(new GitHubCliTokenProvider())
            );
            requestHandlers.TryAdd(MessageMethod.SetLogLevel, new SetLogLevelRequestHandler());

            _logger.LogInformation(
                "Request handlers registered: Initialize, GetOperationClaims, GetAuthenticationCredentials, SetLogLevel"
            );

            _logger.LogDebug("Creating plugin");
            var cancellationTokenSource = new CancellationTokenSource();
            using (
                IPlugin plugin = await PluginFactory
                    .CreateFromCurrentProcessAsync(
                        requestHandlers,
                        ConnectionOptions.CreateDefault(),
                        cancellationTokenSource.Token
                    )
                    .ConfigureAwait(continueOnCapturedContext: false)
            )
            {
                _logger.LogInformation("Plugin created and running, waiting for exit");
                await WaitForPluginExitAsync(plugin, TimeSpan.FromMinutes(2));
                _logger.LogInformation("Plugin shutdown complete");
            }

            return 0;
        }
        catch (OperationCanceledException)
        {
            // When restoring from multiple sources, one of the sources will throw an unhandled TaskCanceledException
            // if it has been restored successfully from a different source.
            _logger.LogInformation(
                "Operation was canceled (this may be expected when restoring from multiple sources)"
            );
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in plugin: {ex.Message}");
            _logger.LogDebug($"Exception details: {ex}");
            await Console.Error.WriteLineAsync($"Error in plugin: {ex.Message}");
            return 1;
        }
    }

    private static async Task WaitForPluginExitAsync(IPlugin plugin, TimeSpan shutdownTimeout)
    {
        var beginShutdownTaskSource = new TaskCompletionSource<object?>();
        var endShutdownTaskSource = new TaskCompletionSource<object?>();

        plugin.BeforeClose += (sender, args) =>
        {
            _logger.LogDebug("Plugin BeforeClose event received");
            beginShutdownTaskSource.TrySetResult(null);
        };

        plugin.Closed += (sender, a) =>
        {
            _logger.LogDebug("Plugin Closed event received");
            beginShutdownTaskSource.TrySetResult(null);
            endShutdownTaskSource.TrySetResult(null);
        };

        await beginShutdownTaskSource.Task;
        _logger.LogDebug($"Waiting for plugin shutdown (timeout: {shutdownTimeout.TotalSeconds}s)");
        using (
            new Timer(
                _ => endShutdownTaskSource.TrySetCanceled(),
                null,
                shutdownTimeout,
                TimeSpan.FromMilliseconds(-1)
            )
        )
        {
            await endShutdownTaskSource.Task;
        }
    }
}
