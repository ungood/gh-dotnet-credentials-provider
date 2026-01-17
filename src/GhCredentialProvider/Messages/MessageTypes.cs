namespace GhCredentialProvider.Messages;

public enum MessageType
{
    None,
    Handshake,
    Initialize,
    GetOperationClaims,
    GetAuthenticationCredentials,
    SetLogLevel,
    Close,
    Log,
    MonitorNuGetProcessExit,
}
