using Newtonsoft.Json;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.Plugin;

public class PluginHost
{
    private readonly JsonRpc _rpc;
    private readonly PluginMessageDispatcher _dispatcher;

    public PluginHost(JsonRpc rpc, PluginMessageDispatcher dispatcher)
    {
        _rpc = rpc;
        _dispatcher = dispatcher;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var requestJson = await _rpc.ReadLineAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(requestJson))
                {
                    break;
                }

                var request = JsonConvert.DeserializeObject<Message>(requestJson);
                if (request == null)
                {
                    continue;
                }

                // Handle Close request immediately
                if (request.Method == MessageMethod.Close)
                {
                    break;
                }

                var response = await _dispatcher.DispatchAsync(request, cancellationToken);
                if (response != null)
                {
                    var responseJson = JsonConvert.SerializeObject(response);
                    await _rpc.WriteLineAsync(responseJson, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // Log error to stderr for debugging
                await Console.Error.WriteLineAsync($"Plugin error: {ex.Message}");
                // Continue to next message instead of breaking
                continue;
            }
        }
    }
}
