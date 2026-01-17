# GitHub NuGet Credential Provider

A NuGet cross-platform credential provider plugin (v2 protocol) that uses the GitHub CLI (`gh`) to provide authentication credentials for GitHub Package feeds.

## Features

- Automatically retrieves GitHub Personal Access Tokens (PATs) using the `gh` CLI
- Supports both GitHub.com and GitHub Enterprise Server
- Falls back to `GH_TOKEN` or `GITHUB_TOKEN` environment variables
- Non-interactive mode support for CI/CD scenarios
- Cross-platform support (Windows, macOS, Linux)

## Requirements

- .NET 10.0 or later
- GitHub CLI (`gh`) installed and authenticated (optional if using environment variables)
- NuGet client that supports cross-platform plugins (NuGet 4.8+)

## Installation

### Option 1: User Plugin Directory (Recommended)

1. Build the project:
   ```bash
   dotnet build -c Release
   ```

2. Create the plugin directory:
   ```bash
   mkdir -p ~/.nuget/plugins/netcore/nuget-plugin-gh
   ```

3. Copy the built DLL:
   ```bash
   cp src/GhCredentialProvider/bin/Release/net10.0/GhCredentialProvider.dll ~/.nuget/plugins/netcore/nuget-plugin-gh/
   ```

   On Windows:
   ```cmd
   mkdir %USERPROFILE%\.nuget\plugins\netcore\nuget-plugin-gh
   copy src\GhCredentialProvider\bin\Release\net10.0\GhCredentialProvider.dll %USERPROFILE%\.nuget\plugins\netcore\nuget-plugin-gh\
   ```

### Option 2: PATH Installation

1. Build the project as a self-contained executable:
   ```bash
   dotnet publish -c Release -r <RID> --self-contained
   ```

2. Rename the executable to match the pattern `nuget-plugin-gh` (lowercase `nuget-plugin-` prefix)

3. Add the directory containing the executable to your PATH

## Configuration

### GitHub CLI Authentication

Ensure the GitHub CLI is authenticated:

```bash
gh auth login
```

Or set environment variables:

```bash
export GH_TOKEN=ghp_your_token_here
# or
export GITHUB_TOKEN=ghp_your_token_here
```

### GitHub Enterprise

The plugin automatically detects GitHub Enterprise Server hosts. Ensure `gh` is configured for your enterprise host:

```bash
gh auth login --hostname ghe.company.com
```

## Usage

Once installed, the plugin will automatically be used by NuGet when accessing GitHub Package feeds. No additional configuration is required.

### Example: Restore packages from GitHub Packages

```bash
dotnet restore --source https://nuget.pkg.github.com/owner/index.json
```

The plugin will:
1. Detect that the source is a GitHub Packages feed
2. Retrieve a token using `gh auth token` or environment variables
3. Provide the token as credentials to NuGet

## Non-Interactive Mode

For CI/CD scenarios where interactive authentication is not possible:

1. Set the `GH_TOKEN` or `GITHUB_TOKEN` environment variable
2. Ensure `NonInteractive=true` is set in your NuGet configuration

The plugin will fail gracefully with a clear error message if no token is available in non-interactive mode.

## How It Works

The plugin implements the NuGet cross-platform plugins protocol v2.0.0:

1. **Handshake**: Negotiates protocol version with NuGet client
2. **Initialize**: Receives client version and settings
3. **GetOperationClaims**: Declares support for "Authentication" operation for GitHub hosts
4. **GetAuthenticationCredentials**: Retrieves PAT and returns it as credentials

The plugin detects GitHub Package feeds by checking for:
- `nuget.pkg.github.com` (GitHub Packages NuGet feed)
- `npm.pkg.github.com` (GitHub Packages npm feed)
- Hostnames containing "github" or "ghe" (GitHub Enterprise)

## Development

### Building

```bash
dotnet build
```

### Testing

```bash
dotnet test
```

### Project Structure

```
src/GhCredentialProvider/
├── Messages/          # Protocol message DTOs
├── Plugin/            # JSON-RPC and message loop
├── Handlers/          # Message handlers
└── GitHub/            # GitHub CLI integration

tests/GhCredentialProvider.Tests/
└── [Test files]
```

## Troubleshooting

### Plugin Not Discovered

- Verify the plugin is installed in the correct directory: `~/.nuget/plugins/netcore/nuget-plugin-gh/`
- Check that the DLL name matches the directory name
- Ensure you're using a NuGet client that supports cross-platform plugins (4.8+)

### Authentication Fails

- Verify `gh auth status` shows you're authenticated
- Check that `GH_TOKEN` or `GITHUB_TOKEN` is set if using environment variables
- Ensure the token has the required scopes (e.g., `read:packages`)

### Non-Interactive Mode Issues

- Set `GH_TOKEN` or `GITHUB_TOKEN` environment variable
- Verify the token is valid and not expired
- Check that `NonInteractive=true` is set in your NuGet configuration

## License

[Specify your license here]

## Contributing

[Contributing guidelines if applicable]
