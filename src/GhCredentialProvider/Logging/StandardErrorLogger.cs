using System;
using System.IO;
using NuGet.Common;

namespace GhCredentialProvider.Logging
{
  internal class StandardErrorLogger : LoggerBase
  {
    private readonly TextWriter _errorWriter;
    private readonly string _className;

    public StandardErrorLogger(string className)
    {
      _errorWriter = Console.Error;
      _className = className;
      // Enable log writes immediately for StandardErrorLogger since it's used for diagnostics
      SetLogLevel(LogLevel.Debug);
    }

    protected override void WriteLog(LogLevel logLevel, string message)
    {
      var level = logLevel.ToString();
      var formattedMessage = $"[{_className}] [{level}] {message}";
      _errorWriter.WriteLine(formattedMessage);
      _errorWriter.Flush();
    }
  }
}
