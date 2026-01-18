using GhCredentialProvider.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NuGet.Common;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.RequestHandlers;

internal abstract class BaseRequestHandler<TRequest, TResponse> : IRequestHandler
    where TResponse : class
{
    private readonly JsonSerializerSettings _serializerSettings;

    protected BaseRequestHandler()
    {
        Logger = new StandardErrorLogger(GetType().Name);
        _serializerSettings = new JsonSerializerSettings { Formatting = Formatting.None };
        _serializerSettings.Converters.Add(new StringEnumConverter());
    }

    protected StandardErrorLogger Logger { get; }

    public virtual CancellationToken CancellationToken { get; private set; } =
        CancellationToken.None;

    /// <summary>
    ///     This is a bit of a silly name for this method... It is actually handling the request from NuGet and returning
    ///     a response.  It delegates to subclasses that implement HandleRequestAsync.
    /// </summary>
    public async Task HandleResponseAsync(
        IConnection connection,
        Message message,
        IResponseHandler responseHandler,
        CancellationToken cancellationToken
    )
    {
        var request = MessageUtilities.DeserializePayload<TRequest>(message);
        var requestType = message.Type.ToString().ToLowerInvariant();
        var payloadString = message.Payload?.ToString(Formatting.None) ?? string.Empty;
        Logger.Log(
            LogLevel.Debug,
            $"Handling {requestType} '{message.Method}': {payloadString}"
        );

        var response = await HandleRequestAsync(request).ConfigureAwait(false);

        // We don't want to print credentials on auth responses
        if (message.Method != MessageMethod.GetAuthenticationCredentials)
        {
            var logResponse = JsonConvert.SerializeObject(response, _serializerSettings);
            Logger.Log(LogLevel.Debug, $"Sending response: {logResponse}");
        }

        await responseHandler
            .SendResponseAsync(message, response, cancellationToken)
            .ConfigureAwait(false);
        CancellationToken = CancellationToken.None;
    }

    public abstract Task<TResponse> HandleRequestAsync(TRequest request);
}