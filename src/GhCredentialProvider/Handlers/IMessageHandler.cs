using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.Handlers;

public interface IMessageHandler
{
    Task<Message> HandleAsync(Message request, CancellationToken cancellationToken = default);
}
