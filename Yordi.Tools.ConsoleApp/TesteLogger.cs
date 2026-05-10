using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Yordi.Tools.ConsoleApp
{
    /// <summary>
    /// Rotinas de teste esporádico do <see cref="LoggerYordi"/>.
    /// Verifica se as mensagens chegam ao console e à janela de Saída do depurador do Visual Studio.
    /// </summary>
    public class TesteLogger : EventBaseClass
    {
        private readonly LoggerYordi _logger;
        private static string diretorio = AppDomain.CurrentDomain.BaseDirectory;

        public TesteLogger()
        {
            _logger = LoggerYordi.LoggerInstance();
            DefineLocalLogger();
        }

        /// <summary>
        /// Executa o teste completo: dispara uma mensagem por nível de log a cada segundo
        /// durante <paramref name="totalSegundos"/> segundos e ao final lança uma exceção de teste.
        /// </summary>
        /// <param name="totalSegundos">Duração total do teste em segundos (padrão: 5).</param>
        public async Task ExecutarAsync(int totalSegundos = 5)
        {

            Message("=== Início do teste de LoggerYordi ===");
            Message($"Depurador anexado: {Debugger.IsAttached}");
            Message(string.Empty);

            var niveis = new[]
            {
                (LogLevel.Information, "Mensagem de INFORMAÇÃO"),
                (LogLevel.Warning,     "Mensagem de AVISO"),
                (LogLevel.Error,       "Mensagem de ERRO (sem exceção)"),
                (LogLevel.Critical,    "Mensagem CRÍTICA (sem exceção)"),
            };

            for (int i = 1; i <= totalSegundos; i++)
            {
                var (nivel, texto) = niveis[(i - 1) % niveis.Length];
                string msg = $"[Tick {i:D2}/{totalSegundos:D2}] {texto}";

                // Usa Write() para capturar a origem (método/linha/arquivo) automaticamente
                _logger.Write(nivel, msg);

                // Também exercita os helpers herdados de EventBaseClass
                if (nivel == LogLevel.Error)
                    Error($"(via EventBaseClass) {msg}");
                else
                    Message($"(via EventBaseClass) {msg}");

                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            // Teste com exceção real
            TestarExcecao();

            Console.WriteLine();
            Console.WriteLine("=== Fim do teste de LoggerYordi ===");
        }

        /// <summary>
        /// Lança e captura uma exceção para verificar o caminho de log de erros com <see cref="Exception"/>.
        /// </summary>
        private void TestarExcecao()
        {
            try
            {
                throw new InvalidOperationException("Exceção de teste lançada intencionalmente pelo TesteLogger.");
            }
            catch (Exception ex)
            {
                // Caminho via ILogger (Log<TState>) — origem capturada via StackFrame
                _logger.Log(LogLevel.Error, new EventId(99, "TesteExcecao"), ex.Message,
                    ex,
                    (s, e) => $"{s} | Exception: {e?.Message}");

                // Caminho via Write() — origem capturada via CallerMemberName
                _logger.Write(LogLevel.Critical, ex.Message, ex);

                // Caminho via EventBaseClass
                Error(ex);
            }
        }
        private static void DefineLocalLogger()
        {
            FileTools.CriaDiretorio(diretorio);
            Logger.Path = FileTools.Combina(diretorio, "Log");
            FileTools.CriaDiretorio(Logger.Path);
            Logger.File = "YordiTools.log";
        }

    }
}
