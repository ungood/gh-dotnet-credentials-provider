using System;
using System.IO;
using System.Threading.Tasks;
using NuGet.Common;

namespace GhCredentialProvider.Logging;

public class StandardErrorLogger : LoggerBase
{
    private readonly TextWriter _errorWriter;
    private readonly string _className;

    public StandardErrorLogger(string className)
        : base()
    {
        _errorWriter = Console.Error;
        _className = className;
    }

    public StandardErrorLogger(string className, LogLevel verbosityLevel)
        : base(verbosityLevel)
    {
        _errorWriter = Console.Error;
        _className = className;
    }

    public override void Log(ILogMessage message)
    {
        if (DisplayMessage(message.Level))
        {
            var formattedMessage = FormatMessage(message);
            _errorWriter.WriteLine(formattedMessage);
            _errorWriter.Flush();
        }
    }

    public override Task LogAsync(ILogMessage message)
    {
        if (DisplayMessage(message.Level))
        {
            var formattedMessage = FormatMessage(message);
            return _errorWriter
                .WriteLineAsync(formattedMessage)
                .ContinueWith(
                    _ => _errorWriter.FlushAsync(),
                    TaskContinuationOptions.ExecuteSynchronously
                )
                .Unwrap();
        }

        return Task.CompletedTask;
    }

    private string FormatMessage(ILogMessage message)
    {
        var level = message.Level.ToString();
        var messageText = message.Message ?? string.Empty;
        return $"[{_className}] [{level}] {messageText}";
    }
}
