# Agent Guidelines: NuGet.Protocol.Plugins

## Implementation Guidance

- Refer to the "NuGet.Client repository" to understand which classes and interfaces are available in
  `NuGet.Protocol.Plugins`. **CRITICAL: Do not duplicate code that already exists in `NuGet.Protocol.Plugins`.**
- Refer to the "TeamCity Credential Provider" as a simple reference implementation.

## Workflow
- Use `mise build` in between changes to ensure they still build.
- Use `mise test` to run unit tests. Whenever reasonable, write failing tests before adding new features.
- Use `mise integ-test` after a large set of changes, before asking the user to review.

## References

Use these references to figure out how to implement functionality:

- [NuGet.Client Repository](https://github.com/NuGet/NuGet.Client) - Source code for `NuGet.Protocol.Plugins`. Consult this to understand available classes, interfaces, and implementation patterns.
- [NuGet Cross-Platform Plugins](https://learn.microsoft.com/en-us/nuget/reference/extensibility/nuget-cross-platform-plugins) - Official documentation for plugin architecture
- [NuGet Authentication Plugin](https://learn.microsoft.com/en-us/nuget/reference/extensibility/nuget-cross-platform-authentication-plugin) - Authentication plugin specification
- [Microsoft Artifacts Credential Provider](https://github.com/microsoft/artifacts-credprovider) - Reference implementation
- [TeamCity Credential Provider](https://github.com/JetBrains/teamcity-nuget-support/blob/master/nuget-extensions/nuget-plugin/Program.cs) - Another reference implementation.
