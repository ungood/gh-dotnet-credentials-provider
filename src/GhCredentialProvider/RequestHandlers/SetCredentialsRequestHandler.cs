using System.Threading.Tasks;
using NuGet.Protocol.Plugins;

namespace GhCredentialProvider.RequestHandlers
{
    /// <summary>
    /// Handles a <see cref="SetCredentialsRequest"/>
    /// </summary>
    internal class SetCredentialsRequestHandler
        : BaseRequestHandler<SetCredentialsRequest, SetCredentialsResponse>
    {
        private static readonly SetCredentialsResponse SuccessResponse = new SetCredentialsResponse(
            MessageResponseCode.Success
        );

        public SetCredentialsRequestHandler() { }

        public override Task<SetCredentialsResponse> HandleRequestAsync(
            SetCredentialsRequest request
        )
        {
            // There's currently no way to handle proxies, so nothing we can do here
            return Task.FromResult(SuccessResponse);
        }
    }
}
