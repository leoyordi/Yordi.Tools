using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;

namespace Yordi.Tools
{
    public static class Logger
    {
        private static string? _file;
        private static string? _fileComplete;
        private static string? _path;
        private static string? _internalFile;
        private static string _firstPath = ".\\Logs";

        // ── Fila thread-safe para serializar todas as escritas em arquivo ──────
        private static readonly Channel<(string texto, string arquivo)> _canal =
            Channel.CreateUnbounded<(string, string)>(new UnboundedChannelOptions
            {
                SingleReader = true,   // um único consumidor → sem lock na escrita
                AllowSynchronousContinuations = false
            });

        private static readonly object _nomeArquivoLock = new();

        static Logger()
        {
            // Consumidor único: lê da fila e grava no arquivo em sequência
            _ = Task.Run(async () =>
            {
                await foreach (var (texto, arquivo) in _canal.Reader.ReadAllAsync())
                {
                    try
                    {
                        FileTools.WriteText(arquivo, texto);
                    }
                    catch { /* evita derrubar o consumer em caso de falha de IO */ }
                }
            });
        }

        /// <summary>
        /// Acrescentar DATA antes da extensão para acrescentar a data do log
        /// <code> NomeArquivoCompleto = "MeuLog_DATA.log"
        /// </code>
        /// </summary>
        public static string? NomeArquivoCompleto
        {
            get
            {
                MontaNomeArquivoCompleto();
                return _fileComplete;
            }
            set
            {
                File = FileTools.NomeArquivo(value);
                var p = FileTools.PastaSomente(value);
                if (!string.IsNullOrEmpty(p))
                    _path = p;
            }
        }
        public static string? File
        {
            get => _file;
            set
            {
                var p = FileTools.NomeArquivoSemExtensao(value) ?? Environment.MachineName;
                var ext = FileTools.Extensao(value) ?? ".log";
                _file = string.Concat(p, "DATA", ext);
            }
        }

        public static string Path { get => _path ?? _firstPath; set => _path = value; }

        public static bool IsConsoleApplication => !Console.IsOutputRedirected && !Console.IsInputRedirected;
        public static string? NomeArquivo(string arquivoCompleto) => FileTools.NomeArquivo(arquivoCompleto);

        public static string? UltimoLog() => FileTools.UltimoLog(_firstPath);
        public static string? LogDiaAnterior() => FileTools.LogDiaAnterior(_firstPath);

        public static async Task<string?> LogAsync(Exception filterContext, string origem = "", int line = 0, string file = "")
        {
            string s = MontaMensagemDeErro(filterContext, origem, line, file);
            if (await GraveAsync(s))
                return s;
            return null;
        }

        public static async Task<string?> LogAsync(string texto, string origem = "", int line = 0, string file = "")
        {
            string s = MontaLinha(texto, origem, line, file);
            if (string.IsNullOrEmpty(s))
                return null;
            if (await GraveAsync(s))
                return s;
            return null;
        }

        private static async Task<bool> GraveAsync(string texto)
        {
            try
            {
                string arquivo = ObterNomeArquivoCompleto();
                await _canal.Writer.WriteAsync((texto, arquivo));
                return true;
            }
            catch { return false; }
        }

        private static string MontaMensagemDeErro(Exception? filterContext, string? origem, int? line, string? file)
        {
            if (filterContext == null)
                return MontaLinha("Exception is null", origem, line, file);

            string s = MontaLinha(filterContext.Message, origem, line, file);
            StringBuilder builder = new StringBuilder(s);
            if (filterContext?.Data != null)
                foreach (DictionaryEntry i in filterContext.Data)
                    builder.AppendLine($" -> {i.Key}: {i.Value}");
            builder.AppendLine(" ===== EXCEPTION ===== ");
            while (filterContext != null)
            {
                builder
                    .AppendLine($"Source: {filterContext.Source}")
                    .AppendLine($"Target: {filterContext.TargetSite}")
                    .AppendLine($"Type: {filterContext.GetType().Name}")
                    .AppendLine($"Stack:")
                    .AppendLine(SimplificaStackTrace(filterContext.StackTrace));
                filterContext = filterContext.InnerException;
                if (filterContext != null)
                {
                    builder.AppendLine("-- INNER EXCEPTION --");
                    builder.AppendLine($"Message: {filterContext.Message}");
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Reduz cada linha do stack trace ao formato resumido:
        /// <c>   at NomeClasse.Metodo() in Arquivo.cs:line N</c>
        /// </summary>
        internal static string SimplificaStackTrace(string? stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace))
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var linha in stackTrace.Split('\n'))
            {
                var l = linha.TrimEnd('\r');

                int inIdx = l.IndexOf(" in ", StringComparison.Ordinal);
                if (inIdx >= 0)
                {
                    string chamada = l[..inIdx].Trim();

                    if (chamada.StartsWith("at ", StringComparison.Ordinal))
                    {
                        string semAt = chamada[3..];
                        var partes = semAt.Split('.');
                        chamada = partes.Length >= 2
                            ? $"at {partes[^2]}.{partes[^1]}"
                            : $"at {semAt}";
                    }

                    string local = l[(inIdx + 4)..].Trim();

                    int barraIdx = Math.Max(local.LastIndexOf('\\'), local.LastIndexOf('/'));
                    if (barraIdx >= 0)
                        local = local[(barraIdx + 1)..];

                    sb.AppendLine($"   {chamada} in {local}");
                }
                else if (!string.IsNullOrWhiteSpace(l))
                {
                    sb.AppendLine($"   {l.Trim()}");
                }
            }
            return sb.ToString();
        }

        public static string? LogSync(Exception filterContext, string? origem = "", int? line = 0, string? file = "")
        {
            string s = MontaMensagemDeErro(filterContext, origem, line, file);
            if (GraveSync(s))
                return s;
            return null;
        }

        public static string? LogSync(string texto, string? origem = "", int? line = 0, string? file = "")
        {
            string s = MontaLinha(texto, origem, line, file);
            if (string.IsNullOrEmpty(s))
                return null;
            if (GraveSync(s))
                return s;
            return null;
        }

        public static string MontaLinha(string texto, string? origem, int? line, string? file)
        {
            // [dd/MM/yyyy HH:mm:ss.fff] já inclui milissegundos
            StringBuilder builder = new StringBuilder($"[{DateTime.Now:dd/MM/yyyy HH:mm:ss.fff}] ");
            bool temOrigem  = !string.IsNullOrEmpty(origem);
            bool temArquivo = !string.IsNullOrEmpty(file);
            bool temLinha   = line.HasValue && line.Value > 0;
            if (temOrigem || temArquivo || temLinha)
            {
                if (temArquivo)
                {
                    var origem2 = $"{FileTools.NomeArquivoSemExtensao(file)}:{origem}";
                    builder.Append(temLinha ? $"[{origem2}:{line}] " : $"[{origem2}] ");
                }
                else
                    builder.Append(temLinha ? $"[{origem}:{line}] " : $"[{origem}] ");
            }

            builder.AppendLine(texto);
            return builder.ToString();
        }

        private static bool GraveSync(string texto)
        {
            try
            {
                string arquivo = ObterNomeArquivoCompleto();
                // Enfileira de forma síncrona; o consumidor serializa a escrita
                _canal.Writer.TryWrite((texto, arquivo));
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Resolve o caminho do arquivo de log de forma thread-safe.
        /// </summary>
        private static string ObterNomeArquivoCompleto()
        {
            lock (_nomeArquivoLock)
            {
                MontaNomeArquivoCompleto();
                return _internalFile!;
            }
        }

        private static void MontaNomeArquivoCompleto()
        {
            if (string.IsNullOrEmpty(_path))
                _path = _firstPath;
            else if (_path.Contains("%TEMP%"))
                _path = _path.Replace("%TEMP%", FileTools.PastaTemporaria());
            if (string.IsNullOrEmpty(_file))
                _file = string.Concat(Environment.MachineName, "DATA.log");

            _internalFile = FileTools.Combina(_path, _file).Replace("DATA", DateTime.Now.ToString("_yyyyMMdd"));
            _fileComplete = FileTools.Combina(_path, _file);
        }
    }
}
