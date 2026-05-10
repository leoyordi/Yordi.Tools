namespace Yordi.Tools.ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ── Teste 1: LoggerYordi (não tipado) ──────────────────────────────────
            // Execute com F5 e verifique a janela Saída > Depurar no Visual Studio.
            var teste = new TesteLogger();
            teste.ExecutarAsync(totalSegundos: 5).GetAwaiter().GetResult();

            Console.WriteLine();
            Console.WriteLine("──────────────────────────────────────────────────");
            Console.WriteLine();

            // ── Teste 2: LoggerYordi<T> (ILogger<T>) ──────────────────────────────
            // Verifica se a categoria do tipo aparece corretamente na origem de cada mensagem.
            var testeT = new TesteLoggerT();
            testeT.ExecutarAsync(totalSegundos: 5).GetAwaiter().GetResult();

            Console.WriteLine();
            Console.WriteLine("Pressione qualquer tecla para sair...");
            Console.ReadKey();
        }
        public class User
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
