// Copyright (c) Microsoft. All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.IO;
using System.Threading.Tasks;
using NuGet.Common;

namespace GhCredentialProvider.Logging;

/// <summary>
/// Logs messages to standard error, with log level included
/// </summary>
public class StandardErrorLogger : LoggerBase
{
    private readonly TextWriter writer;

    public StandardErrorLogger()
    {
        this.writer = Console.Error;
    }

    public override void Log(ILogMessage message)
    {
        if (message == null)
        {
            return;
        }

        writer.WriteLine($"[GH Credential Provider] [{message.Level}] {message.Message}");
        writer.Flush();
    }

    public override Task LogAsync(ILogMessage message)
    {
        Log(message);
        return Task.CompletedTask;
    }
}
