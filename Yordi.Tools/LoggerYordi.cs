using Microsoft.Extensions.Logging;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Yordi.Tools
{
    public class LoggerYordi : ILogger
    {
#pragma warning disable CS8618 // O campo não anulável precisa conter um valor não nulo ao sair do construtor. Considere adicionar o modificador "obrigatório" ou declarar como anulável.
        private static LoggerYordi _log;
#pragma warning restore CS8618 // O campo não anulável precisa conter um valor não nulo ao sair do construtor. Considere adicionar o modificador "obrigatório" ou declarar como anulável.
        public static LoggerYordi LoggerInstance(string path = "")
        {
            _log ??= new LoggerYordi(path);
            return _log;
        }
        private LoggerYordi(string path = "")
        {
            if (!string.IsNullOrEmpty(path) && !string.Equals(path, Logger.Path))
                Logger.Path = path;
        }
        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return new NoopDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel > LogLevel.Debug;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;
            string message = String.Empty;
            if (formatter != null)
            {
                message += formatter(state, exception);
            }
            string log = $"[{DateTime.Now}] [{logLevel}] {message}";
            WriteLine(log, logLevel >= LogLevel.Error);
            Logger.LogSync(log);
        }
        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
        private void WriteLine(string msg, bool isError = false)
        {
#if DEBUG
            if (!isError)
                Console.WriteLine(msg);
            else
                Console.Error.WriteLine(msg);
#endif
        }
    }
    public class LoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string path)
        {
            return LoggerYordi.LoggerInstance(path);
        }

        public void Dispose()
        {
        }
    }

    public static class LoggerYordiExtensions
    {
        public static void LogWarning(this ILogger logger, string message, string? origem = "", int? line = 0, string? file = "")
        {
            WriteLog("WAR", message, origem, line, file);
        }
        public static void LogInformation(this ILogger logger, string message, string? origem = "", int? line = 0, string? file = "")
        {
            WriteLog("INF", message, origem, line, file);
        }

        public static void LogDebug(this ILogger logger, string message, string? origem = "", int? line = 0, string? file = "")
        {
            WriteLog("DEB", message, origem, line, file);
        }
        public static void LogError(this ILogger logger, string message, string? origem = "", int? line = 0, string? file = "")
        {
            WriteLog("ERR", message, origem, line, file);
        }
        public static void LogError(this ILogger logger, Exception e, string? origem = "", int? line = 0, string? file = "")
        {
            string? s = Logger.LogSync(e, origem, line, file);
            WriteConsole(e);
        }
        private static void WriteLog(string typeLog, string message, string? origem = "", int? line = 0, string? file = "")
        {
            string? msg = $"[{typeLog}] {message}";
            msg = Logger.LogSync(msg, origem, line, file);
            WriteConsole(msg);
        }
        private static void WriteConsole(string? msg, bool error = false)
        {
            if (string.IsNullOrEmpty(msg))
                return;
            if (Logger.IsConsoleApplication)
            {
                if (error)
                    Console.Error.WriteLine(msg);
                else
                    Console.WriteLine(msg);
            }
            if (error)
                Debug.Fail(msg + Environment.NewLine);
            else
                Debug.WriteLine(msg);
        }
        private static void WriteConsole(Exception? exception)
        {
            if (Logger.IsConsoleApplication)
                while (exception != null)
                {
                    Console.Error.WriteLine(exception);
                    exception = exception.InnerException;
                }
        }
    }
}
