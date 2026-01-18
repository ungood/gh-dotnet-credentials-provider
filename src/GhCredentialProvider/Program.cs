using GhCredentialProvider.GitHub;
using GhCredentialProvider.Logging;
using GhCredentialProvider.RequestHandlers;
using NuGet.Common;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (
            !string.Equals(args.SingleOrDefault(), "-plugin", StringComparison.OrdinalIgnoreCase)
        )
        {
            Console.Error.WriteLine(
                "Error: This credential provider only supports plugin mode. Expected argument: -plugin"
            );
            return 1;
        }

        var tokenSource = new CancellationTokenSource();
        var logger = new StandardErrorLogger("GhCredentialProvider");

        logger.Log(LogLevel.Verbose, "Entered nuget credentials plugin");

        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            tokenSource.Cancel();
            eventArgs.Cancel = true;
        };

        try
        {
            return MainInternal(tokenSource).GetAwaiter().GetResult();
        }
        catch (OperationCanceledException e)
        {
            // Multiple source restoration. Request will be cancelled if a package has been successfully restored from another source
            logger.Log(
                LogLevel.Verbose,
                $"Request to credential provider was cancelled. Message: {e.Message}"
            );
            return 0;
        }
    }

    private static async Task<int> MainInternal(CancellationTokenSource tokenSource)
    {
        var logger = new StandardErrorLogger("GhCredentialProvider");
        var tokenProvider = new GitHubCliTokenProvider();
        var requestHandlers = new NuGet.Protocol.Plugins.RequestHandlers();
        requestHandlers.TryAdd(
            MessageMethod.GetAuthenticationCredentials,
            new GetAuthenticationCredentialsHandler(tokenProvider)
        );
        requestHandlers.TryAdd(MessageMethod.GetOperationClaims, new GetOperationClaimsHandler());
        requestHandlers.TryAdd(MessageMethod.SetLogLevel, new SetLogLevelHandler());
        requestHandlers.TryAdd(MessageMethod.Initialize, new InitializeHandler());

        logger.Log(LogLevel.Verbose, "Running in plug-in mode");

        using var plugin = await PluginFactory
            .CreateFromCurrentProcessAsync(
                requestHandlers,
                ConnectionOptions.CreateDefault(),
                CancellationToken.None
            )
            .ConfigureAwait(false);
        await RunNuGetPluginsAsync(plugin, tokenSource.Token).ConfigureAwait(false);

        return 0;
    }

    private static async Task RunNuGetPluginsAsync(
        IPlugin plugin,
        CancellationToken cancellationToken
    )
    {
        var logger = new StandardErrorLogger("GhCredentialProvider");
        SemaphoreSlim semaphore = new(0);

        plugin.Connection.Faulted += (sender, a) =>
        {
            logger.Log(
                LogLevel.Error,
                $"Faulted on message: {a.Message?.Type} {a.Message?.Method} {a.Message?.RequestId}"
            );
            logger.Log(LogLevel.Error, a.Exception.ToString());
        };

        plugin.Closed += (sender, a) => semaphore.Release();

        var complete = await semaphore
            .WaitAsync(TimeSpan.FromDays(1), cancellationToken)
            .ConfigureAwait(false);

        if (!complete) logger.Log(LogLevel.Error, "Timed out waiting for plug-in operations to complete");
    }
}