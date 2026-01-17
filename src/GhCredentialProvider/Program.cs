using GhCredentialProvider.GitHub;
using GhCredentialProvider.Handlers;
using GhCredentialProvider.Plugin;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Check if running as a plugin
        if (args.Length == 0 || args[0] != "-Plugin")
        {
            Console.WriteLine("GitHub NuGet Credential Provider");
            Console.WriteLine("This is a NuGet cross-platform plugin. It should be invoked by NuGet client tools.");
            Console.WriteLine("For more information, see: https://learn.microsoft.com/en-us/nuget/reference/extensibility/nuget-cross-platform-plugins");
            return 1;
        }

        try
        {
            // Create token provider
            var tokenProvider = new GitHubCliTokenProvider();

            // Create handlers - route by request payload type
            var handlers = new Dictionary<Type, IMessageHandler>
            {
                { typeof(HandshakeRequest), new HandshakeHandler() },
                { typeof(InitializeRequest), new InitializeHandler() },
                { typeof(GetOperationClaimsRequest), new GetOperationClaimsHandler() },
                { typeof(GetAuthenticationCredentialsRequest), new GetAuthenticationCredentialsHandler(tokenProvider) },
                { typeof(SetLogLevelRequest), new SetLogLevelHandler() }
            };

            // Create dispatcher
            var dispatcher = new PluginMessageDispatcher(handlers);

            // Create JSON-RPC layer using stdin/stdout
            var rpc = new JsonRpc(Console.OpenStandardInput(), Console.OpenStandardOutput());

            // Create and run plugin host
            var host = new PluginHost(rpc, dispatcher);
            await host.RunAsync();

            return 0;
        }
        catch (Exception ex)
        {
            // Log to stderr (NuGet protocol allows logging to stderr)
            await Console.Error.WriteLineAsync($"Error in plugin: {ex.Message}");
            return 1;
        }
    }
}
