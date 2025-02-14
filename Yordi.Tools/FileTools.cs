using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Yordi.Tools
{
    public static class FileTools
    {
        private static EventBaseClass baseClass = new EventBaseClass();
        static object locker = new object();
        private static string? _msg;
        public static string? Mensagem { get { return _msg; } }

        public static string ConfigFolder 
        {
            get
            {
                if (string.IsNullOrEmpty(_configFolder))
                    return ".\\Configs"; // Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
                else if (_configFolder.StartsWith(".\\"))
                    return Path.Combine(AppFolder, "Configs");
                else
                    return _configFolder;
            }
            set => _configFolder = value; 
        }

        private static string _configFolder = ".\\Configs";


        public static string BDFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_bdFolder))
                    return ".\\Database"; // Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
                else if (_bdFolder.StartsWith(".\\"))
                    return Path.Combine(AppFolder, "Database");
                else
                    return _bdFolder;
            }
            set => _bdFolder = value;
        }

        private static string _bdFolder = ".\\Database";


        public static string AppFolder
        {
            get
            {
                if (PastaExiste(AppDomain.CurrentDomain.BaseDirectory))
                    return AppDomain.CurrentDomain.BaseDirectory;
                return Environment.CurrentDirectory;
            }
        }

        #region Eventos
        //public static event MyMessage RetornoEvent;
        //public static event MyProgress ProgressoEvent;
        //public static event MyRows RegistrosQtiEvent;
        //public static event MyError ErroEvent;
        static internal void Message(string mensagem, [CallerMemberName] string origem = "", [CallerLineNumber] int line = 0)
        {
            baseClass.Message(mensagem, origem, line);
        }
        static internal void Error(string mensagem, [CallerMemberName] string origem = "", [CallerLineNumber] int line = 0)
        {
            baseClass.Error(mensagem, origem, line);
        }
        static internal void Error(Exception e, [CallerMemberName] string origem = "", [CallerLineNumber] int line = 0)
        {
            baseClass.Error(e, origem, line);
        }
        static internal void Rows(float registros) { baseClass.Rows(registros); }
        static internal void Progresso(float progresso) { baseClass.Progresso(progresso); }

        #endregion



        public static async Task<string> ReadAllTextAsync(string filePath, Encoding? encoding = null)
        {
            _msg = String.Empty;
            var stringBuilder = new StringBuilder();
            if (encoding == null)
                encoding = Encoding.UTF8;

            using (var fileStream = File.OpenRead(filePath))
            using (var streamReader = new StreamReader(fileStream, encoding))
            {
                string? line = await streamReader.ReadLineAsync();
                while (line != null)
                {
                    stringBuilder.AppendLine(line);
                    line = await streamReader.ReadLineAsync();
                }
                return stringBuilder.ToString();
            }
        }
        private const int NumberOfRetries = 3;
        private const int DelayOnRetry = 1000;


        public static string? ReadAllText(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            Rows(fileInfo.Length);
            try
            {
                string[]? AllText = null;
                using (StreamReader sr = File.OpenText(filePath))
                {
                    AllText = new string[sr.BaseStream.Length];    //only allocate memory here
                    int x = 0;
                    while (!sr.EndOfStream)
                    {
                        string? line = sr.ReadLine();
                        if (line != null)
                        {
                            AllText[x] = line;
                            x++;
                        }
                        Progresso(sr.BaseStream.Position);
                    }
                } //CLOSE THE FILE because we are now DONE with it.
                Rows(AllText.Length);
                StringBuilder sb = new StringBuilder();
                Parallel.For(0, AllText.Length, x =>
                {
                    Progresso(x);
                    sb.Append(AllText[x]); //to simulate work
                });
                GC.Collect();
                return sb.ToString();
            }
            catch (OutOfMemoryException outOfMemory)
            {
                Error(outOfMemory);
            }
            catch (Exception e)
            {
                Error(e);
            }
            GC.Collect();
            return null;
        }
        
        /// <summary>
        /// Lê todo o arquivo e retorna o conteúdo como string.
        /// https://stackoverflow.com/questions/26741191/ioexception-the-process-cannot-access-the-file-file-path-because-it-is-being#answer-26741192
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ReadAllText(string filePath, Encoding? encoding = null)
        {
            _msg = String.Empty;
            string s = String.Empty;
            if (ArquivoExiste(filePath))
            {
                for (int i = 1; i <= NumberOfRetries; ++i)
                {
                    try
                    {
                        Stream fs = File.OpenRead(filePath);
                        if (encoding == null)
                            encoding = Encoding.GetEncoding(DetectFileEncoding(fs));

                        using (var reader = new StreamReader(fs, encoding))
                        {
                            s = reader.ReadToEnd();
                        }
                        //s = File.ReadAllText(filePath, encoding);
                        break; // When done we can break loop
                    }
                    catch (IOException) when (i <= NumberOfRetries)
                    {
                        // You may check error code to filter some exceptions, not every error
                        // can be recovered.
                        Thread.Sleep(DelayOnRetry);
                    }
                    catch (Exception ec)
                    {
                        _msg = ec.Message;
                    }
                }
            }
            else
                _msg = $"{NomeArquivo(filePath)} does not exist.";
            return s;
        }

        /// <summary>
        /// Lê todo o arquivo e armazena as linhas que houver numa array de strings
        /// https://stackoverflow.com/questions/26741191/ioexception-the-process-cannot-access-the-file-file-path-because-it-is-being#answer-26741192
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string[]? ReadAllLines(string filePath, Encoding? encoding = null)
        {
            _msg = string.Empty;
            string[]? lines = null;
            if (ArquivoExiste(filePath))
            {
                for (int i = 1; i <= NumberOfRetries; ++i)
                {
                    try
                    {
                        encoding ??= DetectFileEncoding(filePath);
                        lines = File.ReadAllLines(filePath, encoding);
                        break; // When done we can break loop
                    }
                    catch (IOException) when (i <= NumberOfRetries)
                    {
                        // You may check error code to filter some exceptions, not every error
                        // can be recovered.
                        Thread.Sleep(DelayOnRetry);
                    }
                    catch (Exception ec)
                    {
                        _msg = ec.Message;
                    }
                }
            }
            else
                _msg = $"{NomeArquivo(filePath)} does not exist.";
            return lines;
        }

        public static bool WriteText(string? filePath, string text, Encoding? encoding = null, bool replace = false)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            byte[] encodedText;
            try
            {
                encoding ??= Encoding.UTF8;

                encodedText = encoding.GetBytes(text);

                return WriteAllBytes(filePath, encodedText, replace);
            }
            catch (Exception e)
            {
                try
                {
                    encoding ??= Encoding.UTF8;
                    encodedText = encoding.GetBytes(e.Message);
                    string p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"{System.Diagnostics.Process.GetCurrentProcess().ProcessName}.log");
                    WriteAllBytes(p, encodedText, replace);
                }
                catch { }
                return false; 
            }
        }

        public static bool WriteAllBytes(string filePath, byte[] bytes, bool replace = false)
        {
            try
            {
                FileMode mode = FileMode.Append;
                if (replace) mode = FileMode.Create;
                using (FileStream sourceStream = new FileStream(filePath,
                    mode, FileAccess.Write, FileShare.ReadWrite,
                    bufferSize: 4096, useAsync: false))
                {
                    lock (locker)
                    {
                        sourceStream.Write(bytes, 0, bytes.Length);
                        //sourceStream.Flush();
                    }
                };
                return true;
            }
            catch { return false; }
        }

        public static async Task WriteTextAsync(string? filePath, string text, Encoding? encoding = null)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            encoding ??= Encoding.UTF8;

            byte[] encodedText = encoding.GetBytes(text);

            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.Append, FileAccess.Write, FileShare.ReadWrite,
                bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
                //sourceStream.Flush();
            };
        }


        private static string DetectFileEncoding(Stream fileStream)
        {
            var Utf8EncodingVerifier = Encoding.GetEncoding("utf-8", new EncoderExceptionFallback(), new DecoderExceptionFallback());
            using (var reader = new StreamReader(fileStream, Utf8EncodingVerifier,
                   detectEncodingFromByteOrderMarks: true, leaveOpen: true, bufferSize: 1024))
            {
                string detectedEncoding;
                try
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                    }
                    detectedEncoding = reader.CurrentEncoding.BodyName;
                }
                catch (Exception)
                {
                    // Failed to decode the file using the BOM/UT8. 
                    // Assume it's local ANSI
                    detectedEncoding = "ISO-8859-1";
                }
                // Rewind the stream
                fileStream.Seek(0, SeekOrigin.Begin);
                return detectedEncoding;
            }
        }

        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to ASCII when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="filename">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        public static Encoding DetectFileEncoding(string filename, Encoding? defaultEncoding = null)
        {
            // Read the BOM
            var bom = new byte[4];
            Encoding? encoding = null;
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) encoding = Encoding.UTF8;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) encoding = Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) encoding = Encoding.UTF32; //UTF-32LE
            if (bom[0] == 0xff && bom[1] == 0xfe) encoding = Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) encoding = Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) encoding = new UTF32Encoding(true, true);  //UTF-32BE

            if (encoding == null)
            {
                using (Stream fs = File.OpenRead(filename))
                    encoding = Encoding.GetEncoding(DetectFileEncoding(fs));
            }
            // We actually have no idea what the encoding is if we reach this point, so
            // you may wish to return null instead of defaulting to ASCII
            return encoding ?? defaultEncoding ?? Encoding.ASCII;
        }


        #region Derivados

        /// <summary>Retorna o nome de arquivo e a extensão da cadeia de caracteres do caminho especificado.
        /// </summary>
        /// <param name="filePath">
        /// A cadeia de caracteres do caminho do qual o nome do arquivo e a extensão serão obtidos.
        /// </param>
        /// <returns>Os caracteres após o último caractere de diretório em path. Se o último caractere de path for um 
        /// caractere de separador de diretório ou volume, esse método retornará System.String.Empty. 
        /// Se path for null, esse método retornará null.
        /// </returns>
        /// <exception cref="e">T:System.ArgumentException:
        /// path contém um ou mais caracteres inválidos definidos em System.IO.Path.GetInvalidPathChars.
        /// </exception>
        public static string? NomeArquivo(string? filePath) => Path.GetFileName(filePath);
        public static string? PastaSomente(string? nomeArquivoCompleto) 
            => string.IsNullOrEmpty(nomeArquivoCompleto) ? null : new DirectoryInfo(nomeArquivoCompleto)?.Parent?.Name;
        public static string PastaTemporaria() => Path.GetTempPath();

        public static string? NomeArquivoSemExtensao(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return null;
            return Path.GetFileNameWithoutExtension(filePath);
        }

        public static string? Extensao(string? filePath)
        {
            if (string.IsNullOrEmpty (filePath)) return null;
            return Path.GetExtension(filePath);
        }
        public static DateTime? GetBuildDate()
        {
            UriBuilder uri = new UriBuilder(Assembly.GetExecutingAssembly().Location); //.CodeBase - obsoleto
            string unescapePath = Uri.UnescapeDataString(uri.Path);
            if (string.IsNullOrEmpty(unescapePath))
                return null;
            string? path = Path.GetDirectoryName(unescapePath);
            if (string.IsNullOrEmpty(path))
                return null;
            return File.GetLastWriteTime(path);
        }
        public static bool ArquivoExiste(string arquivo) => File.Exists(arquivo);
        public static bool PastaExiste(string pasta) => Directory.Exists(pasta);
        public static bool CriaDiretorio(string path)
        {
            if (PastaExiste(path)) return true;
            try
            {
                Directory.CreateDirectory(path);
                return true;
            }
            catch { return false; }
        }

        public static string Combina(string pasta, string arquivo) => Path.Combine(pasta, arquivo);
        public static IEnumerable<string> Pastas(string path)
        {
            DirectoryInfo? directoryInfo = Directory.GetParent(path);
            while (directoryInfo != null)
            {
                string s = directoryInfo.Name;
                directoryInfo = Directory.GetParent(directoryInfo.FullName);
                yield return s;
            }
        }

        public static string? UltimoLog(string pasta)
        {
            var arquivos = ArquivosPorData(pasta);
            if (arquivos == null || arquivos.Length == 0) return null;
            var arq = arquivos[0];
            return arq.FullName;
        }
        public static string? LogDiaAnterior(string pasta)
        {
            var arquivos = ArquivosPorData(pasta);
            if (arquivos == null || arquivos.Length == 0)
                return null;
            var arq = arquivos.FirstOrDefault(m => m.LastWriteTime.Date < DateTime.Now.Date);
            if (arq != null)
                return arq.FullName;
            return null;
        }

        /// <summary>
        /// Traz os arquivos .log da pasta informada em ordem decrescente da data de escrita/atualização
        /// </summary>
        /// <param name="pasta"></param>
        /// <returns></returns>
        private static FileInfo[]? ArquivosPorData(string pasta)
        {
            DirectoryInfo diretorio = new DirectoryInfo(pasta);
            FileInfo[] arquivos = diretorio.GetFiles("*.log", SearchOption.TopDirectoryOnly);
            if (arquivos == null || arquivos.Length == 0)
                return null;
            Array.Sort(arquivos, delegate (FileInfo a, FileInfo b) { return DateTime.Compare(b.LastWriteTime, a.LastWriteTime); });
            return arquivos;
        }

        private static FileInfo[]? ArquivosPorExtensao(string pasta, string extensao)
        {
            try
            {
                DirectoryInfo diretorio = new DirectoryInfo(pasta);
                return diretorio.GetFiles(extensao, SearchOption.AllDirectories);
            }
            catch (Exception e)
            {
                Error(e);
            }
            return null;
        }
        public static IEnumerable<string>? ListarArquivos(string pasta, string criterio)
        {
            try
            {
                return Directory.EnumerateFiles(pasta, criterio, SearchOption.AllDirectories);
            }
            catch (Exception e)
            {
                Error(e);
            }
            return null;
        }

        public static string? Excluir(string arquivo)
        {
            try { File.Delete(arquivo); return null; }
            catch (Exception e) { return e.Message; }
        }

        public static int Excluir(string pasta, string extensao, DateTime olderThan)
        {
            var files = ArquivosPorExtensao(pasta, extensao);
            if (files == null || files.Length == 0)
                return 0;
            var filter = files.Where(m => m.CreationTime < olderThan);
            int r = 0;
            foreach (var file in filter)
            {
                try
                {
                    file.Delete();
                    r++;
                }
                catch (Exception) { }
            }
            return r;
        }

        /// <summary>
        /// Move o arquivo de origem para o arquivo de destino.
        /// Se o destino existir, ele será substituído.
        /// O arquivo de origem será excluído.
        /// </summary>
        /// <param name="arquivoOrigem"></param>
        /// <param name="arquivoDestino"></param>
        /// <returns></returns>
        public static bool Mover(string arquivoOrigem, string arquivoDestino)
        {
            try
            {
                if (File.Exists(arquivoDestino))
                    File.Delete(arquivoDestino);
                File.Move(arquivoOrigem, arquivoDestino);
                return true;
            }
            catch (Exception e) { Error(e); return false; }
        }

        public static DateTime? DataCriacao(string arq)
        {
            try
            {
                var file = new FileInfo(arq);
                return file.CreationTime;
            }
            catch { return null; }
        }

        #endregion
    }
}
