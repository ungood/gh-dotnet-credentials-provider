using System;
using System.IO;
using NuGet.Common;

namespace GhCredentialProvider.Logging
{
  internal class StandardErrorLogger
  {
    private readonly TextWriter _errorWriter;
    private readonly string _className;
    private LogLevel _minLogLevel = LogLevel.Debug;

    public StandardErrorLogger(string className)
    {
      _errorWriter = Console.Error;
      _className = className;
    }

    public void Log(LogLevel level, string message)
    {
      if (level >= _minLogLevel)
      {
        var levelString = level.ToString();
        var formattedMessage = $"[{_className}] [{levelString}] {message}";
        _errorWriter.WriteLine(formattedMessage);
        _errorWriter.Flush();
      }
    }

    public void SetLogLevel(LogLevel newLogLevel)
    {
      _minLogLevel = newLogLevel;
    }
  }
}
