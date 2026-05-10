using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Yordi.Tools.ConsoleApp
{
    /// <summary>
    /// Rotinas de teste esporádico do <see cref="LoggerYordi{T}"/> (ILogger&lt;T&gt;).
    /// Verifica se a categoria do tipo aparece corretamente na origem de cada mensagem
    /// e se as mensagens chegam ao console e à janela de Saída do depurador do Visual Studio.
    /// </summary>
    public class TesteLoggerT : EventBaseClass
    {
        // Logger tipado — equivalente ao ILogger<TesteLoggerT> do .NET
        private readonly LoggerYordi<TesteLoggerT> _logger;

        public TesteLoggerT()
        {
            _logger = LoggerYordi.Instance<TesteLoggerT>();
        }

        /// <summary>
        /// Executa o teste completo do logger tipado: dispara uma mensagem por nível de log
        /// a cada segundo durante <paramref name="totalSegundos"/> segundos e ao final lança
        /// uma exceção de teste.
        /// </summary>
        /// <param name="totalSegundos">Duração total do teste em segundos (padrão: 5).</param>
        public async Task ExecutarAsync(int totalSegundos = 5)
        {
            _logger.Write(LogLevel.Information, $"=== Início do teste de LoggerYordi<T> ===");
            _logger.Write(LogLevel.Information, $"Categoria: {typeof(TesteLoggerT).Name}");
            _logger.Write(LogLevel.Information, $"Depurador anexado: {Debugger.IsAttached}");

            var niveis = new[]
            {
                (LogLevel.Information, "Mensagem de INFORMAÇÃO  [tipada]"),
                (LogLevel.Warning,     "Mensagem de AVISO       [tipada]"),
                (LogLevel.Error,       "Mensagem de ERRO        [tipada, sem exceção]"),
                (LogLevel.Critical,    "Mensagem CRÍTICA        [tipada, sem exceção]"),
            };

            for (int i = 1; i <= totalSegundos; i++)
            {
                var (nivel, texto) = niveis[(i - 1) % niveis.Length];
                string msg = $"[Tick {i:D2}/{totalSegundos:D2}] {texto}";

                // Write() — origem capturada via CallerMemberName + categoria prefixada
                _logger.Write(nivel, msg);

                // ILogger<T>.Log<TState>() — origem capturada via StackFrame
                ((ILogger<TesteLoggerT>)_logger).Log(nivel, new EventId(i, "TesteT"), msg,
                    null,
                    (s, _) => $"(via ILogger<T>) {s}");

                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            // Teste com exceção real
            TestarExcecao();

            _logger.Write(LogLevel.Information, "=== Fim do teste de LoggerYordi<T> ===");
        }

        /// <summary>
        /// Lança e captura uma exceção para verificar o caminho de log de erros com
        /// <see cref="Exception"/> através do logger tipado.
        /// </summary>
        private void TestarExcecao()
        {
            try
            {
                throw new InvalidOperationException("Exceção de teste lançada intencionalmente pelo TesteLoggerT.");
            }
            catch (Exception ex)
            {
                // Write() com exceção — origem capturada via CallerMemberName
                _logger.Write(LogLevel.Error, ex.Message, ex);

                // ILogger<T>.Log<TState>() com exceção — origem capturada via StackFrame
                ((ILogger<TesteLoggerT>)_logger).Log(
                    LogLevel.Critical,
                    new EventId(99, "TesteExcecaoT"),
                    ex.Message,
                    ex,
                    (s, e) => $"(via ILogger<T>) {s} | Exception: {e?.Message}");
            }
        }
    }
}
