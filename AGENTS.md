# Agent Guidelines: NuGet.Protocol.Plugins

**CRITICAL: Do not duplicate code that already exists in `NuGet.Protocol.Plugins`.**

## Repository Reference

- **[NuGet.Client Repository](https://github.com/NuGet/NuGet.Client)** - Contains the `NuGet.Protocol.Plugins` source code. Use this repository to understand available classes, interfaces, and implementation patterns.

## Implementation Guidance

Refer to the [NuGet.Client repository](https://github.com/NuGet/NuGet.Client) to understand which classes and interfaces are available in `NuGet.Protocol.Plugins`. Use the repository's source code and examples to determine the appropriate classes and patterns for your implementation needs.

## What Framework Handles Automatically

- Handshake negotiation (DO NOT register Handshake handler)
- JSON serialization/deserialization
- stdin/stdout I/O and connection lifecycle
- Message routing and timeout management

## What You Must Implement

Implement `IRequestHandler` for: `Initialize`, `GetOperationClaims`, `GetAuthenticationCredentials`, `SetLogLevel`.

## References

Use these references to figure out how to implement functionality:

- **[NuGet.Client Repository](https://github.com/NuGet/NuGet.Client)** - Source code for `NuGet.Protocol.Plugins`. Consult this to understand available classes, interfaces, and implementation patterns.
- [NuGet Cross-Platform Plugins](https://learn.microsoft.com/en-us/nuget/reference/extensibility/nuget-cross-platform-plugins) - Official documentation for plugin architecture
- [NuGet Authentication Plugin](https://learn.microsoft.com/en-us/nuget/reference/extensibility/nuget-cross-platform-authentication-plugin) - Authentication plugin specification
- [Microsoft Artifacts Credential Provider](https://github.com/microsoft/artifacts-credprovider) - Reference implementation
