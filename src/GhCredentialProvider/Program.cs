using System.Diagnostics;
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
        DebugBreakIfPluginDebuggingIsEnabled();

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
            return MainInternal(tokenSource, args).GetAwaiter().GetResult();
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

    private static async Task<int> MainInternal(
        CancellationTokenSource tokenSource,
        string[] args
    )
    {
        var logger = new StandardErrorLogger("GhCredentialProvider");
        var tokenProvider = new GitHubCliTokenProvider();
        var sdkInfo = new SdkInfo();
        var requestHandlers = new NuGet.Protocol.Plugins.RequestHandlers();
        requestHandlers.TryAdd(
            MessageMethod.GetAuthenticationCredentials,
            new GetAuthenticationCredentialsRequestHandler(tokenProvider)
        );
        requestHandlers.TryAdd(
            MessageMethod.GetOperationClaims,
            new GetOperationClaimsRequestHandler(sdkInfo)
        );
        requestHandlers.TryAdd(MessageMethod.SetLogLevel, new SetLogLevelHandler());
        requestHandlers.TryAdd(MessageMethod.Initialize, new InitializeRequestHandler());
        requestHandlers.TryAdd(
            MessageMethod.SetCredentials,
            new SetCredentialsRequestHandler()
        );

        if (
            string.Equals(args.SingleOrDefault(), "-plugin", StringComparison.OrdinalIgnoreCase)
        )
        {
            logger.Log(LogLevel.Verbose, "Running in plug-in mode");

            try
            {
                using var plugin = await PluginFactory
                    .CreateFromCurrentProcessAsync(
                        requestHandlers,
                        ConnectionOptions.CreateDefault(),
                        CancellationToken.None
                    )
                    .ConfigureAwait(false);
                await RunNuGetPluginsAsync(plugin, tokenSource.Token)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException e)
            {
                // Multiple source restoration. Request will be cancelled if a package has been successfully restored from another source
                logger.Log(
                    LogLevel.Verbose,
                    $"Request to credential provider was cancelled. Message: {e.Message}"
                );
            }

            return 0;
        }

        if (
            requestHandlers.TryGet(
                MessageMethod.GetAuthenticationCredentials,
                out var requestHandler
            )
            && requestHandler
                is GetAuthenticationCredentialsRequestHandler getAuthenticationCredentialsRequestHandler
        )
        {
            logger.Log(LogLevel.Verbose, "Running in stand-alone mode");

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: gh-dotnet-credential-provider <NuGetFeedUrl>");
                return 1;
            }

            var request = new GetAuthenticationCredentialsRequest(
                new Uri(args[0]),
                false,
                true,
                false
            );
            var response = getAuthenticationCredentialsRequestHandler
                .HandleRequestAsync(request)
                .GetAwaiter()
                .GetResult();

            Console.WriteLine(response?.Username);
            Console.WriteLine(response?.Password);
            Console.WriteLine(response?.Password?.ToJsonWebTokenString());

            return 0;
        }

        return -1;
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

    private static void DebugBreakIfPluginDebuggingIsEnabled()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NUGET_PLUGIN_DEBUG")))
            while (!Debugger.IsAttached)
                Thread.Sleep(100);
    }
}