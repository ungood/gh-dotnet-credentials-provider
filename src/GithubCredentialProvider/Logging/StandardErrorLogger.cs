using NuGet.Common;

namespace GithubCredentialProvider.Logging;

internal class StandardErrorLogger(string className)
{
    private readonly string _className = className;
    private readonly TextWriter _errorWriter = Console.Error;
    private LogLevel _minLogLevel = LogLevel.Debug;

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