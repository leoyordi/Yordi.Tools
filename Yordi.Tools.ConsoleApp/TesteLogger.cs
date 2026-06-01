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

                _logger.Write(nivel, msg);

                if (nivel == LogLevel.Error)
                    Error($"(via EventBaseClass) {msg}");
                else
                    Message($"(via EventBaseClass) {msg}");

                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            TestarExcecao();

            Console.WriteLine();
            Console.WriteLine("=== Fim do teste de LoggerYordi ===");
        }

        /// <summary>
        /// Executa o teste de concorrência: várias threads escrevem simultaneamente no log
        /// para verificar se as entradas não se misturam.
        /// </summary>
        /// <param name="totalThreads">Quantidade de threads simultâneas (padrão: 8).</param>
        /// <param name="mensagensPorThread">Mensagens que cada thread enviará (padrão: 10).</param>
        public async Task ExecutarConcurrenteAsync(int totalThreads = 8, int mensagensPorThread = 10)
        {
            Console.WriteLine();
            Console.WriteLine($"=== Teste de Concorrência: {totalThreads} threads × {mensagensPorThread} msgs ===");
            Message($"[CONCORRÊNCIA] Iniciando {totalThreads} threads simultâneas...");

            var niveis = new[] { LogLevel.Information, LogLevel.Warning, LogLevel.Error, LogLevel.Critical };
            var sw = Stopwatch.StartNew();

            // Cria todas as tasks de uma vez — sem await entre elas para forçar concorrência real
            var tasks = Enumerable.Range(1, totalThreads).Select(threadId => Task.Run(async () =>
            {
                for (int i = 1; i <= mensagensPorThread; i++)
                {
                    var nivel = niveis[(threadId + i) % niveis.Length];
                    string msg = $"[Thread {threadId:D2} | Msg {i:D2}/{mensagensPorThread:D2}] " +
                                 $"nível={nivel} tick={sw.ElapsedMilliseconds}ms";

                    // Exercita os dois caminhos de escrita em paralelo
                    _logger.Write(nivel, msg);

                    if (nivel >= LogLevel.Error)
                        Error($"(EventBaseClass) {msg}");
                    else
                        Message($"(EventBaseClass) {msg}");

                    // Pequeno delay aleatório para embaralhar a ordem de chegada
                    await Task.Delay(Random.Shared.Next(10, 80));
                }
            })).ToArray();

            await Task.WhenAll(tasks);

            sw.Stop();
            Message($"[CONCORRÊNCIA] Concluído em {sw.ElapsedMilliseconds}ms. " +
                    $"Total esperado: {totalThreads * mensagensPorThread} entradas.");
            Console.WriteLine("=== Fim do teste de Concorrência ===");
            Console.WriteLine();

            // Aguarda o canal esvaziar antes de continuar (consumidor é async)
            await Task.Delay(300);
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
                _logger.Log(LogLevel.Error, new EventId(99, "TesteExcecao"), ex.Message,
                    ex,
                    (s, e) => $"{s} | Exception: {e?.Message}");

                _logger.Write(LogLevel.Critical, ex.Message, ex);

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
