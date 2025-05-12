using System.Security.Cryptography;
using System.Text;

namespace Yordi.Tools
{
    public class Cripto
    {
        #region Variáveis e Métodos Privados
        private string _key;
        private SymmetricAlgorithm _algorithm;
        private string? _msg;
        /// <summary>
        /// Inicialização do vetor do algoritmo simétrico
        /// </summary>
        private void SetIV()
        {
            _algorithm.IV = new byte[] { 0xf, 0x6f, 0x13, 0x2e, 0x35, 0xc2, 0xcd, 0xf9, 0x5, 0x46, 0x9c, 0xea, 0xa8, 0x4b, 0x73, 0xcc };
        }

        private void RedefineKey()
        {
            // Ajusta o tamanho da chave se necessário e retorna uma chave válida
            if (_algorithm.LegalKeySizes.Length > 0)
            {
                // Tamanho das chaves em bits
                int keySize = _key.Length * 8;
                int minSize = _algorithm.LegalKeySizes[0].MinSize;
                int maxSize = _algorithm.LegalKeySizes[0].MaxSize;
                int skipSize = _algorithm.LegalKeySizes[0].SkipSize;
                if (keySize > maxSize)
                {
                    // Busca o valor máximo da chave
                    _key = _key.Substring(0, maxSize / 8);
                }
                else if (keySize < maxSize)
                {
                    // Seta um tamanho válido
                    int validSize = (keySize <= minSize) ? minSize : (keySize - keySize % skipSize) + skipSize;
                    if (keySize < validSize)
                    {
                        // Preenche a chave com arterisco para corrigir o tamanho
                        var padding = validSize / 8;
                        _key = _key.PadRight(padding, '*');
                    }
                }
            }
        }
        #endregion
        #region Properties
        public string? Mensagem { get { return _msg; } }

        #endregion
        /// <summary>
        /// Contrutor em que é setado um tipo de criptografia padrão (Rijndael).
        /// </summary>
        public Cripto(string key)
        {
            _key = key;
            _algorithm = Aes.Create();
            _algorithm.Mode = CipherMode.CBC;
            RedefineKey();
        }
        
        #region Public methods
        /// <summary>
        /// Gera a chave de criptografia válida dentro do array.
        /// </summary>
        /// <returns>Chave com array de bytes.</returns>
        public virtual byte[] GetKey()
        {
            string salt = string.Empty;
            PasswordDeriveBytes key = new PasswordDeriveBytes(_key, ASCIIEncoding.ASCII.GetBytes(salt));

            return key.GetBytes(_key.Length);
        }
        /// <summary>
        /// Encripta o dado solicitado.
        /// </summary>
        /// <param name="plainText">Texto a ser criptografado.</param>
        /// <returns>Texto criptografado.</returns>
        public virtual string Encrypt(string texto)
        {
            byte[] plainByte = Encoding.UTF8.GetBytes(texto);
            byte[] keyByte = GetKey();
            // Seta a chave privada
            _algorithm.Key = keyByte;
            SetIV();
            // Interface de criptografia / Cria objeto de criptografia
            ICryptoTransform cryptoTransform = _algorithm.CreateEncryptor();
            MemoryStream _memoryStream = new MemoryStream();
            CryptoStream _cryptoStream = new CryptoStream(_memoryStream, cryptoTransform, CryptoStreamMode.Write);
            // Grava os dados criptografados no MemoryStream
            _cryptoStream.Write(plainByte, 0, plainByte.Length);
            _cryptoStream.FlushFinalBlock();
            // Busca o tamanho dos bytes encriptados
            byte[] cryptoByte = _memoryStream.ToArray();
            // Converte para a base 64 string para uso posterior em um xml
            return Convert.ToBase64String(cryptoByte, 0, cryptoByte.GetLength(0));
        }
        /// <summary>
        /// Desencripta o dado solicitado.
        /// </summary>
        /// <param name="cryptoText">Texto a ser descriptografado.</param>
        /// <returns>Texto descriptografado.</returns>
        public virtual string? Decrypt(string textoCriptografado)
        {
            // Converte a base 64 string em num array de bytes
            byte[] cryptoByte;
            try
            {
                cryptoByte = Convert.FromBase64String(textoCriptografado);
            }
            catch (ArgumentNullException)
            {
                _msg = "Texto inválido, vazio ou nulo";
                return null;
            }
            catch (FormatException)
            {
                _msg = "Texto em formato inválido";
                return null;
            }

            byte[] keyByte = GetKey();
            // Seta a chave privada
            _algorithm.Key = keyByte;
            SetIV();
            // Interface de criptografia / Cria objeto de descriptografia
            ICryptoTransform cryptoTransform = _algorithm.CreateDecryptor();
            try
            {
                MemoryStream _memoryStream = new MemoryStream(cryptoByte, 0, cryptoByte.Length);
                CryptoStream _cryptoStream = new CryptoStream(_memoryStream, cryptoTransform, CryptoStreamMode.Read);
                // Busca resultado do CryptoStream
                StreamReader _streamReader = new StreamReader(_cryptoStream);
                var x = _streamReader.ReadToEnd();
                return x;
            }
            catch (Exception ex)
            {
                _msg = ex.Message;
                return null;
            }
        }
        #endregion
    }

    public static class SenhaNova
    {
        private const string CaracteresValidos = "1234567890QWERTYUIOPASDFGHJKLÇZXCVBNM 1234567890abcdefghijklmnopqrstuvwxyz1234567890@#!?-:+/=1234567890";
        private const string LetrasENumeros = "1234567890QWERTYUIOPASDFGHJKLZXCVBNM1234567890abcdefghijklmnopqrstuvwxyz12345678901234567890";

        public static string CriaSenha(int tamanho)
        {
            int valormaximo = CaracteresValidos.Length;

            Random random = new Random(DateTime.Now.Millisecond);

            StringBuilder senha = new StringBuilder(tamanho);

            for (int indice = 0; indice < tamanho; indice++)
                senha.Append(CaracteresValidos[random.Next(0, valormaximo)]);

            return senha.ToString();
        }
        public static string CriaSenhaFraca(int tamanho)
        {
            int valormaximo = LetrasENumeros.Length;

            Random random = new Random(DateTime.Now.Millisecond);

            StringBuilder senha = new StringBuilder(tamanho);

            for (int indice = 0; indice < tamanho; indice++)
                senha.Append(LetrasENumeros[random.Next(0, valormaximo)]);

            return senha.ToString();
        }

    }
}
