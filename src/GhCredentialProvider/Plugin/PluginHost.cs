using GhCredentialProvider.Logging;
using Newtonsoft.Json;
using NuGet.Common;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.Plugin;

public class PluginHost
{
    private readonly JsonRpc _rpc;
    private readonly PluginMessageDispatcher _dispatcher;
    private readonly ILogger _logger;

    public PluginHost(JsonRpc rpc, PluginMessageDispatcher dispatcher, ILogger logger)
    {
        _rpc = rpc;
        _dispatcher = dispatcher;
        _logger = logger;
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

                // Log received message
                _logger.LogVerbose($"Received message: {requestJson}");

                var request = JsonConvert.DeserializeObject<Message>(requestJson);
                if (request == null)
                {
                    _logger.LogWarning("Failed to deserialize message");
                    continue;
                }

                // Handle Close request immediately
                if (request.Method == MessageMethod.Close)
                {
                    _logger.LogInformation("Received Close request, shutting down");
                    break;
                }

                var response = await _dispatcher.DispatchAsync(request, cancellationToken);
                if (response != null)
                {
                    var responseJson = JsonConvert.SerializeObject(response);
                    
                    // Log sent message
                    _logger.LogVerbose($"Sent message: {responseJson}");
                    
                    await _rpc.WriteLineAsync(responseJson, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // Log error using logger
                _logger.LogError($"Plugin error: {ex.Message}");
                _logger.LogVerbose($"Exception details: {ex}");
                // Continue to next message instead of breaking
                continue;
            }
        }
    }
}
