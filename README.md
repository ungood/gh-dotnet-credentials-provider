# Github Nuget Credential Provider

A NuGet cross-platform credential provider plugin that uses the GitHub CLI (`gh`) to authenticate with GitHub Package feeds.

## Features

- Automatically retrieves GitHub tokens using the `gh` CLI
- Supports GitHub.com and GitHub Enterprise Server
- Falls back to `GH_TOKEN` or `GITHUB_TOKEN` environment variables
- Cross-platform support (Windows, macOS, Linux)

## Requirements

- .NET 10.0 or later
- GitHub CLI (`gh`) installed and authenticated (optional if using environment variables)
- NuGet 4.8+ with cross-platform plugin support

## Installation

Install from NuGet:
```bash
dotnet tool install -g nuget-plugin-github-credential-provider
```

## Configuration

Authenticate with GitHub CLI:
```bash
gh auth login
```

Or for GitHub Enterprise:
```bash
gh auth login --hostname ghe.company.com
```

Or set an environment variable:
```bash
export GITHUB_TOKEN=ghp_your_token_here
```

## Usage

The plugin automatically provides credentials when accessing GitHub Package feeds:

```bash
dotnet restore --source https://nuget.pkg.github.com/example/index.json
```