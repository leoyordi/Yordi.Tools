namespace Yordi.Tools.ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var user = new
            {
                Username = "Yordi",
                Password = "123456"
            };
            var cripto = new Cripto("PalavraChave");
            string s1 = cripto.Encrypt("23456");
            string s2 = cripto.Encrypt(user.Password);
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.ReadKey();
        }
        public class User
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
