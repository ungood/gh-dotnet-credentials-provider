using System.Text.Json.Serialization;

namespace GhCredentialProvider.Messages;

public class Request
{
    [JsonPropertyName("RequestType")]
    public MessageType RequestType { get; set; }

    [JsonPropertyName("RequestId")]
    public string RequestId { get; set; } = string.Empty;
}
