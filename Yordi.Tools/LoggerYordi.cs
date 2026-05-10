using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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

        /// <summary>
        /// Retorna um logger singleton tipado para <typeparamref name="T"/>,
        /// equivalente ao <see cref="ILogger{T}"/> do .NET.
        /// <code>
        /// var log = LoggerYordi.Instance&lt;MinhaClasse&gt;();
        /// log.Write(LogLevel.Information, "Olá!");
        /// // saída: [MinhaClasse.MeuMetodo:42] [INF] Olá!
        /// </code>
        /// </summary>
        public static LoggerYordi<T> Instance<T>() => LoggerYordi<T>.LoggerInstance();
        protected LoggerYordi(string path = "")
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

        /// <summary>
        /// Registra uma mensagem capturando automaticamente o método, linha e arquivo do chamador.
        /// Prefira este método ao <see cref="ILogger.Log{TState}"/> para ter a origem preenchida.
        /// </summary>
        public void Write(LogLevel logLevel, string message, Exception? exception = null,
            [CallerMemberName] string origem = "",
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = "")
        {
            if (!IsEnabled(logLevel))
                return;

            WriteCore(logLevel, message, exception, origem, line, file);
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

            // Origem capturada via StackFrame: sobe 1 frame além do Log<TState>
            var frame = new StackFrame(1, needFileInfo: true);
            string origem = frame.GetMethod()?.Name ?? "";
            int line      = frame.GetFileLineNumber();
            string file   = frame.GetFileName() ?? "";

            WriteCore(logLevel, message, exception, origem, line, file);
        }

        private void WriteCore(LogLevel logLevel, string message, Exception? exception,
            string origem, int line, string file)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    this.LogTrace(message, origem, line, file);
                    break;
                case LogLevel.Debug:
                    this.LogDebug(message, origem, line, file);
                    break;
                case LogLevel.Information:
                    this.LogInformation(message, origem, line, file);
                    break;
                case LogLevel.Warning:
                    this.LogWarning(message, origem, line, file);
                    break;
                case LogLevel.Error:
                    if (exception != null)
                        this.LogError(exception, origem, line, file);
                    else
                        this.LogError(message, origem, line, file);
                    break;
                case LogLevel.Critical:
                    if (exception != null)
                        this.LogCritical(exception, origem, line, file);
                    else
                        this.LogCritical(message, origem, line, file);
                    break;
                case LogLevel.None:
                    break;
                default:
                    this.LogInformation(message, origem, line, file);
                    break;
            }

            string log = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss.fff}] [{logLevel}] {message}";
            WriteLine(log, logLevel >= LogLevel.Error);
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
            // Escreve na janela de saída do depurador do Visual Studio quando habilitado
            if (Debugger.IsAttached)
                Debug.WriteLine(msg);
        }
    }

    /// <summary>
    /// Logger genérico tipado equivalente ao <see cref="ILogger{T}"/> do .NET.
    /// O nome da categoria (<typeparamref name="T"/>) é automaticamente prefixado em cada mensagem.
    /// </summary>
    public sealed class LoggerYordi<T> : LoggerYordi, ILogger<T>
    {
        private static readonly string _categoria = typeof(T).Name;

#pragma warning disable CS8618
        private static LoggerYordi<T> _instance;
#pragma warning restore CS8618

        private LoggerYordi() { }

        /// <summary>
        /// Retorna a instância singleton tipada para <typeparamref name="T"/>.
        /// </summary>
        public new static LoggerYordi<T> LoggerInstance(string path = "")
        {
            _instance ??= new LoggerYordi<T>();
            return _instance;
        }

        /// <summary>
        /// Registra uma mensagem prefixando o nome de <typeparamref name="T"/> na origem.
        /// </summary>
        public new void Write(LogLevel logLevel, string message, Exception? exception = null,
            [CallerMemberName] string origem = "",
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = "")
        {
            // Prefixa a categoria para identificar claramente a classe dona do log
            base.Write(logLevel, message, exception, $"{_categoria}.{origem}", line, file);
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
        public static void LogCritical(this ILogger logger, string message, string? origem = "", int? line = 0, string? file = "")
        {
            WriteLog("CRI", message, origem, line, file);
        }
        public static void LogCritical(this ILogger logger, Exception e, string? origem = "", int? line = 0, string? file = "")
        {
            string? s = Logger.LogSync(e, origem, line, file);
            WriteConsole(e);
        }

        public static void LogTrace(this ILogger logger, string message, string? origem = "", int? line = 0, string? file = "")
        {
            WriteLog("TRA", message, origem, line, file);
        }
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
            if (!msg.EndsWith(Environment.NewLine))
                msg += Environment.NewLine;
            if (Logger.IsConsoleApplication)
            {
                if (error)
                    Console.Error.Write(msg);
                else
                    Console.Write(msg);
            }

            if (Debugger.IsAttached)
            {
                if (error)
                    Debug.Fail(msg);
                else
                    Debug.Write(msg);
            }
        }
        private static void WriteConsole(Exception? exception)
        {
            while (exception != null)
            {
                string header  = $"{exception.GetType().Name}: {exception.Message}";
                string stack   = Logger.SimplificaStackTrace(exception.StackTrace);
                string saida   = $"{header}{Environment.NewLine}{stack}";

                if (Logger.IsConsoleApplication)
                {
                    Console.Error.WriteLine(saida);
                    //Console.Error.WriteLine(exception);
                }

                if (Debugger.IsAttached)
                {
                    Debug.WriteLine(saida);
                    //Debug.WriteLine(exception);
                }

                exception = exception.InnerException;
                if (exception != null)
                    Debug.WriteLine("-- INNER EXCEPTION --");
            }
        }
    }
}
