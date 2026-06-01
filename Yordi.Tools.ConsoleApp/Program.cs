using Yordi.Tools.ConsoleApp;

namespace Yordi.Tools.ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ── Teste 1: LoggerYordi (não tipado) ─────────────────────────────────
            var teste = new TesteLogger();
            teste.ExecutarAsync(totalSegundos: 5).GetAwaiter().GetResult();

            Console.WriteLine();
            Console.WriteLine("──────────────────────────────────────────────────");
            Console.WriteLine();

            // ── Teste 2: Concorrência — múltiplas threads escrevendo ao mesmo tempo ─
            // Verifica se o Channel serializa corretamente sem misturar entradas.
            teste.ExecutarConcurrenteAsync(totalThreads: 8, mensagensPorThread: 10)
                 .GetAwaiter().GetResult();

            Console.WriteLine();
            Console.WriteLine("──────────────────────────────────────────────────");
            Console.WriteLine();

            // ── Teste 3: LoggerYordi<T> (ILogger<T>) ──────────────────────────────
            var testeT = new TesteLoggerT();
            testeT.ExecutarAsync(totalSegundos: 5).GetAwaiter().GetResult();

            Console.WriteLine();
            Console.WriteLine("Pressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        public class User
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }
    }
}
