using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Yordi.Tools
{
    public class DBConfig
    {
        public string? Name { get; set; }

        //public string ConnectionString 
        //{ 
        //    get => connectionString; 
        //    set => connectionString = value; 
        //}

        public string? Local
        {
            get => local;
            set
            {
                //if (value.ToUpper().Contains("PROGRAMDATA"))
                //    local = value.Replace("%PROGRAMDATA%", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
                //else
                    local = value;
            }
        }
        public string? Database { get; set; }
        public int? Porta { get; set; }
        public string? User { get; set; }
        public string? Password { get; set; }
        public string? Adicional { get; set; }
        public int TryReconnect { get; set; }
        public int SecondsWaitToTry { get; set; }

        public bool VerificaTabelas { get; set; }


        private string openName = "[";
        private string closeName = "]";
        private TipoDB _tipo = TipoDB.SQLite;
        private string? local;
        private string? connectionString;

        public string OpenName { get { return openName; } }
        public string CloseName { get { return closeName; } }

        public TipoDB TipoDB
        {
            get { return _tipo; }
            set
            {
                //openName = "`";

                if (value == TipoDB.MySQL)
                {
                    openName = "`";
                    closeName = openName;
                }
                else
                {
                    openName = "[";
                    closeName = "]";
                }

                _tipo = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether verbose logging is enabled.
        /// </summary>
        public bool? Verbose { get; set; } = false;

        /// <summary>
        /// Monta string de conexão baseado nas propiedades da classe. Substitui a propriedade ConnectionString
        /// Caso a propriedade Local ou Database sejam nulas ou vazias, a propriedade ConnectionString não será substituída 
        /// e o valor dela é que será retornada
        /// </summary>
        /// <returns>String de conexão</returns>
        public string? StringDeConexaoMontada()
        {
            if (string.IsNullOrEmpty(Local) || string.IsNullOrEmpty(Database))
                return connectionString;
            StringBuilder s = new StringBuilder();
            if (_tipo == TipoDB.MySQL)
            {
                //"server=localhost;port=3306;uid=root;pwd=Password;database=MyDB;convert zero datetime=True",
                s.Append($"server={Local};");
                s.Append($"database={Database};");

                if (Porta.HasValue && Porta.Value > 0)
                    s.Append($"port={Porta};");
                else
                    s.Append("port=3306;");
                if (!string.IsNullOrEmpty(User))
                    s.Append($"uid={User};");
                if (!string.IsNullOrEmpty(Password))
                    s.Append($"pwd={Password};");

                s.Append(Adicional);
            }
            else if (_tipo == TipoDB.SQLite)
            {
                s.Append($"Data Source={(Path.Combine(Local, Database))};");
                s.Append(Adicional);
            }
            connectionString = s.ToString();
            return connectionString;
        }
    }

    public static class ControleAlteracao
    {
        public static int UsuarioID { get; set; }
        public static string? Usuario { get; set; }
        public static string? Origem { get; set; }
    }
}
