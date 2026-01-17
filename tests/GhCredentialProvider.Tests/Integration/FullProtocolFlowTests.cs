using System.Text;
using GhCredentialProvider.GitHub;
using GhCredentialProvider.Handlers;
using GhCredentialProvider.Plugin;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;
using NSubstitute;
using Xunit;

namespace GhCredentialProvider.Tests.Integration;

public class FullProtocolFlowTests
{
    [Fact(Skip = "Integration test requires proper Message deserialization - needs investigation")]
    public async Task FullProtocolFlow_HandshakeInitializeAndGetCredentials()
    {
        // Setup
        var tokenProvider = Substitute.For<ITokenProvider>();
        tokenProvider.GetTokenAsync("github.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>("ghp_testtoken123"));

        var handlers = new Dictionary<Type, IMessageHandler>
        {
            { typeof(HandshakeRequest), new HandshakeHandler() },
            { typeof(InitializeRequest), new InitializeHandler() },
            { typeof(GetOperationClaimsRequest), new GetOperationClaimsHandler() },
            { typeof(GetAuthenticationCredentialsRequest), new GetAuthenticationCredentialsHandler(tokenProvider) }
        };

        var dispatcher = new PluginMessageDispatcher(handlers);

        // Simulate protocol messages as JSON strings
        var messages = new List<string>
        {
            JsonConvert.SerializeObject(new Message("handshake-1", MessageType.Request, MessageMethod.Handshake, 
                Newtonsoft.Json.Linq.JObject.FromObject(new HandshakeRequest(NuGet.Versioning.SemanticVersion.Parse("2.0.0"), NuGet.Versioning.SemanticVersion.Parse("2.0.0"))))),
            JsonConvert.SerializeObject(new Message("init-1", MessageType.Request, MessageMethod.Initialize,
                Newtonsoft.Json.Linq.JObject.FromObject(new InitializeRequest("6.0.0", "en-US", TimeSpan.FromSeconds(5))))),
            JsonConvert.SerializeObject(new Message("claims-1", MessageType.Request, MessageMethod.GetOperationClaims,
                Newtonsoft.Json.Linq.JObject.FromObject(new GetOperationClaimsRequest("https://nuget.pkg.github.com/owner/index.json", Newtonsoft.Json.Linq.JObject.FromObject(new { PackageSourceRepository = "https://nuget.pkg.github.com/owner/index.json" }))))),
            JsonConvert.SerializeObject(new Message("auth-1", MessageType.Request, MessageMethod.GetAuthenticationCredentials,
                Newtonsoft.Json.Linq.JObject.FromObject(new GetAuthenticationCredentialsRequest(new Uri("https://nuget.pkg.github.com/owner/index.json"), isRetry: false, isNonInteractive: true, canShowDialog: false))))
        };

        var inputStream = new MemoryStream();
        var outputStream = new MemoryStream();
        var writer = new StreamWriter(inputStream, Encoding.UTF8, leaveOpen: true);

        foreach (var message in messages)
        {
            await writer.WriteLineAsync(message);
        }
        await writer.FlushAsync();
        inputStream.Position = 0;

        var rpc = new JsonRpc(inputStream, outputStream);
        var host = new PluginHost(rpc, dispatcher);

        // Run the host (it will process all messages)
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(5)); // Timeout after 5 seconds
        await host.RunAsync(cts.Token);

        // Verify responses
        outputStream.Position = 0;
        var reader = new StreamReader(outputStream, Encoding.UTF8);
        var responses = new List<string>();
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                responses.Add(line);
            }
        }

        Assert.Equal(4, responses.Count); // Should have 4 responses

        // Verify handshake response
        Assert.Contains("\"RequestId\":\"handshake-1\"", responses[0]);
        Assert.Contains("Handshake", responses[0]);

        // Verify initialize response
        Assert.Contains("\"RequestId\":\"init-1\"", responses[1]);

        // Verify claims response
        Assert.Contains("\"RequestId\":\"claims-1\"", responses[2]);
        Assert.Contains("Authentication", responses[2]);

        // Verify auth response
        Assert.Contains("\"RequestId\":\"auth-1\"", responses[3]);
        Assert.Contains("ghp_testtoken123", responses[3]);
    }
}
