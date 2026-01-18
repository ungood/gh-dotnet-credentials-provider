# GitHub NuGet Credential Provider

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

TODO: Publish to nuget so this can be installed as a tool.

`mise install` - Note that this only works on Mac and Linux for now.  

## Configuration

Authenticate with GitHub CLI:
```bash
gh auth login
```

Or set an environment variable:
```bash
export GH_TOKEN=ghp_your_token_here
```

For GitHub Enterprise:
```bash
gh auth login --hostname ghe.company.com
```

## Usage

The plugin automatically provides credentials when accessing GitHub Package feeds:

```bash
dotnet restore --source https://nuget.pkg.github.com/owner/index.json
```