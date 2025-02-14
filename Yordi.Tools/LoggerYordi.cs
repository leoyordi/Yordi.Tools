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
            WriteLine(log);
            Logger.LogSync(log);
        }
        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
        private void WriteLine(string msg)
        {
#if DEBUG
            Console.WriteLine(msg);
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
        public static void LogWarning(this ILogger logger, string message, 
            [CallerMemberName] string origem = "",
            [CallerLineNumber] int line = 0)
        {
            // logger.Log(LogLevel.Critical, message, callerMemberName, line);
            Logger.LogSync(message, origem, line);
            WriteLine($"[{DataPadrao.Brasilia}] [WRN] [{origem}:{line}] {message}");
        }
        public static void LogInformation(this ILogger logger, string message,
            [CallerMemberName] string origem = "",
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = "")
        {
            string origem2;
            if (string.IsNullOrEmpty(file))
                origem2 = origem;
            else
                origem2 = $"{FileTools.NomeArquivoSemExtensao(file)}:{origem}";
            Logger.LogSync(message, origem2, line);
            WriteLine($"[{DataPadrao.Brasilia}] [INF] [{origem2}:{line}] {message}");
        }

        public static void LogDebug(this ILogger logger, string message,
            [CallerMemberName] string origem = "",
            [CallerLineNumber] int line = 0)
        {
            Logger.LogSync(message, origem, line);
            WriteLine($"[{DataPadrao.Brasilia}] [DEB] [{origem}:{line}] {message}");
        }
        public static void LogError(this ILogger logger, string message,
            [CallerMemberName] string origem = "",
            [CallerLineNumber] int line = 0)
        {
            Logger.LogSync(message, origem, line);
            WriteLine($"[{DataPadrao.Brasilia}] [ERR] [{origem}:{line}] {message}");
        }
        public static void LogError(this ILogger logger, Exception e, 
            [CallerMemberName] string origem = "", 
            [CallerLineNumber] int line = 0)
        {
            Logger.LogSync(e, origem, line);
            WriteLine($"[{DataPadrao.Brasilia}] [ERR] [{origem}:{line}] {e.Message}");
            if (e.Data.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (DictionaryEntry i in e.Data)
                {
                    sb.AppendLine($"{i.Key}: {i.Value}");
                }
                WriteLine(sb.ToString());
            }
            WriteLine(e);
        }
        private static void WriteLine(string msg, bool error = false)
        {
            if (Logger.IsConsoleApplication)
                Console.WriteLine(msg);
            Debug.WriteLine(msg);
        }
        private static void WriteLine(Exception? exception)
        {
            if (Logger.IsConsoleApplication)
                while (exception != null)
                {
                    Console.WriteLine(exception);
                    exception = exception.InnerException;
                }
        }
    }
}
