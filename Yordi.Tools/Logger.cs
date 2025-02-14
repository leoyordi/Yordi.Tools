using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;

namespace Yordi.Tools
{
    public static class Logger
    {
        private static string? _file;
        private static string? _fileComplete;
        private static string? _path;
        private static string? _internalFile;
        private static string _firstPath = ".\\Logs"; // Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

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
        public static async Task LogAsync(Exception filterContext, [CallerMemberName] string origem = "", [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = "")
        {
            string origem2;
            if (string.IsNullOrEmpty(file))
                origem2 = origem;
            else
                origem2 = $"{FileTools.NomeArquivoSemExtensao(file)}:{origem}";
            string builder = MontaMensagemDeErro(filterContext, origem2, line);
            await GraveAsync(builder);
        }


        public static async Task LogAsync(string texto, [CallerMemberName] string origem = "", [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = "")
        {
            string origem2;
            if (string.IsNullOrEmpty(file))
                origem2 = origem;
            else
                origem2 = $"{FileTools.NomeArquivoSemExtensao(file)}:{origem}";
            string s = $"[{DataPadrao.Brasilia}] [{origem2}:{line}] {texto}{Environment.NewLine}";
            await GraveAsync(s);
        }
        private static async Task GraveAsync(string texto)
        {
            try
            {
                MontaNomeArquivoCompleto();
                await FileTools.WriteTextAsync(_internalFile, texto);
            }
            catch { }
        }


        private static string MontaMensagemDeErro(Exception? filterContext, string origem, int line)
        {
            StringBuilder builder = new StringBuilder();
            builder
                .AppendLine(DataPadrao.Brasilia.ToString())
                .AppendLine($"Origem: {origem}:{line}");
            if (filterContext?.Data != null)
            foreach (DictionaryEntry i in filterContext.Data)
                builder.AppendLine($"{i.Key}: {i.Value}");
            while (filterContext != null)
            {
                builder
                    .AppendLine($"Message: {filterContext.Message}")
                    .AppendLine($"Source: {filterContext.Source}")
                    .AppendLine($"Target: {filterContext.TargetSite}")
                    .AppendLine($"Type: {filterContext.GetType().Name}")
                    .AppendLine($"Stack: {filterContext.StackTrace}")
                    ;
                filterContext = filterContext.InnerException;
                if (filterContext != null)
                    builder.AppendLine("INNER EXCEPTION");
            }
            return builder.ToString();
        }
        public static bool LogSync(Exception filterContext, [CallerMemberName] string origem = "", [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = "")
        {
            string origem2;
            if (string.IsNullOrEmpty(file))
                origem2 = origem;
            else
                origem2 = $"{FileTools.NomeArquivoSemExtensao(file)}:{origem}";
            string erro = MontaMensagemDeErro(filterContext, origem2, line);

            return GraveSync(erro);
        }

        public static bool LogSync(string texto, [CallerMemberName] string origem = "", [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = "")
        {
            string origem2;
            if (string.IsNullOrEmpty(file))
                origem2 = origem;
            else
                origem2 = $"{FileTools.NomeArquivoSemExtensao(file)}:{origem}";
            string s = $"[{DataPadrao.Brasilia}] [{origem2}:{line}] {texto}{Environment.NewLine}";
            return GraveSync(s);
        }

        private static bool GraveSync(string texto)
        {
            try
            {
                MontaNomeArquivoCompleto();
                return FileTools.WriteText(_internalFile, texto);
            }
            catch { return false; }
        }

        private static void MontaNomeArquivoCompleto()
        {
            if (string.IsNullOrEmpty(_path))
                _path = _firstPath;
            else if (_path.Contains("%TEMP%"))
                _path = _path.Replace("%TEMP%", FileTools.PastaTemporaria());
            if (string.IsNullOrEmpty(_file))
                _file = string.Concat(Environment.MachineName, "DATA.log");

            _internalFile = FileTools.Combina(_path, _file).Replace("DATA", DataPadrao.Brasilia.ToString("_yyyyMMdd"));

            _fileComplete = FileTools.Combina(_path, _file);
        }
    }
}
