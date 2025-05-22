using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Yordi.Tools
{
    public static class Conversores
    {
        public static DateTime ToDataHora(string expressao)
        {
            DateTime data = DateTime.MinValue;
            if (DateTime.TryParse(expressao, out data))
                return data;
            return data;
        }
        public static DateTime ToDataHora(object objeto)
        {
            DateTime data = DateTime.MinValue;
            if (DateTime.TryParse(objeto.ToString(), out data))
                return data;
            return data;
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="dataHora"></param>
        /// <returns>yyyy-MM-dd HH:mm:ss</returns>
        public static string DateToString(DateTime dataHora)
        {
            string teste = dataHora.ToString("yyyy-MM-dd HH:mm:ss");
            DateTime dt;
            if (DateTime.TryParse(teste, out dt))
                return teste;
            else
                return String.Empty;
            //yyyy-MM-dd HH:mm:ss
        }

        public static decimal ToDecimal(string? expressao)
        {
            if (Decimal.TryParse(expressao, out decimal valor))
                return valor;
            return valor;
        }
        public static decimal ToDecimal(object? expressao)
        {
            decimal valor = 0;
            if (expressao == null)
                return valor;
            if (decimal.TryParse(expressao.ToString(), out valor))
                return valor;
            return valor;
        }

        public static double ToDouble(string? expressao)
        {
            double valor = 0;
            if (Double.TryParse(expressao, out valor))
                return valor;
            return valor;
        }

        public static double ToDouble(object? expressao)
        {
            double valor = 0;
            if (expressao == null)
                return valor;
            if (double.TryParse(expressao.ToString(), out valor))
                return valor;
            return valor;
        }

        #region ToInt
        public static int ToInt(string? expressao)
        {
            if (int.TryParse(expressao, out int valor))
                return valor;
            return valor;
        }
        public static int ToInt(object? expressao)
        {
            int valor = 0;
            if (expressao == null)
                return valor;
            if (int.TryParse(expressao.ToString(), out valor))
                return valor;
            return valor;
        }
        public static int ToInt(bool valor)
        {
            if (valor)
                return 1;
            else
                return 0;
        }
        public static int? ToInt(this bool? valor)
        {
            if (!valor.HasValue)
                return null;
            if (valor.Value)
                return 1;
            return 0;
        }
        #endregion

        #region ToBool
        public static bool ToBool(int valor)
        {
            if (valor == 1 || valor == -1)
                return true;
            else
                return false;
        }
        public static bool ToBool(string? expressao)
        {
            if (string.IsNullOrEmpty(expressao))
                return false;
            var lower = expressao.ToLower();
            switch(lower)
            {
                case "verdadeiro":
                case "true":
                case "verdadeira":
                case "y":
                case "s":
                case "1":
                case "-1":
                    return true;
                default:
                    return false;
            }
        }
        public static bool ToBool(object? objeto)
        {
            return ToBool(objeto?.ToString());
        }
        public static bool? ToBool(this int? valor)
        {
            if (!valor.HasValue)
                return null;
            return ToBool(valor.Value);
        }
        #endregion

        public static Tipo PropriedadeTipo(PropertyInfo p)
        {
            var type = p.PropertyType;
            var underlyingType = Nullable.GetUnderlyingType(type);
            var r = underlyingType ?? type;

            if (r.IsEnum)
                return Tipo.ENUM;
            else
            {
                if (r == typeof(byte[]))
                {
                    return Tipo.BLOB;
                }
                else
                {
                    switch (r.Name)
                    {
                        case "String":
                            return Tipo.STRING;
                        case "Int32":
                        case "Int64":
                            return Tipo.INT;
                        case "Decimal":
                            return Tipo.MONEY;
                        case "Double":
                            return Tipo.DOUBLE;
                        case "DateTime":
                            return Tipo.DATA;
                        case "TimeSpan":
                            return Tipo.HORA;
                        case "Boolean":
                            return Tipo.BOOL;
                        case "Guid":
                            return Tipo.GUID;
                        default:
                            return Tipo.STRING;
                    }
                }
            }
        }

        public static string Right(string value, int size)
        {
            // if length is greater than "size" resets "size"(se comprimento é maior que "tamanho" redefine"tamanho")
            size = (value.Length < size ? value.Length : size);
            string newValue = value.Substring(value.Length - size);
            return newValue;
        }

        public static string SubstituiDiacritico(this string texto)
        {
            string comAcentos = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇçñÑ";
            string semAcentos = "AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCcnN";

            for (int i = 0; i < comAcentos.Length; i++)
            {
                texto = texto.Replace(comAcentos[i], semAcentos[i]);
            }
            return texto;
        }
        public static string RemoveCaracteresEspeciais(
            this string texto,
            bool aceitaEspaco = true, 
            bool substituiAcentos = false, 
            bool substituiPontos = true)
        {            
            if (string.IsNullOrEmpty(texto))
                return texto;

            if (substituiAcentos)
                texto.SubstituiDiacritico();

            if (substituiPontos)
                texto.RemovePontos();

            if (!aceitaEspaco)
                texto = texto.Replace(" ", string.Empty);

            return texto;
        }
        public static string RemovePontos(this string texto)
        {
            if (String.IsNullOrEmpty(texto))
                return String.Empty;
            else
                return texto.Replace(".", String.Empty);
        }


        public static Version? ToVersion(string texto)
        {
            var split = texto.Split('.');
            if (split == null || split.Length == 0) return null;
            Version? result = null;
            try
            {
                Regex regexObj = new Regex(@"[\D]");
                for (int i = 0; i < split.Length; i++)
                {
                    split[i] = regexObj.Replace(split[i], "");
                }
                string s = string.Join(".", split);
                if (Version.TryParse(s, out result))
                    return result;
            }
            catch { }
            return result;
        }

        /// <summary>
        /// Retorna apenas os números de uma expressão. Inclui ponto.
        /// </summary>
        /// <param name="texto"></param>
        /// <returns></returns>
        public static string RetornaNumeros(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return texto;
            return new string(texto.Where(m => m.Equals('.') || char.IsDigit(m)).ToArray());
        }


        #region Json
        public static string ToJson<T>(T? obj, bool writeIndent = false)
        {
            var type = typeof(T);
            if (obj == null) return string.Empty;
            try
            {
                string r = JsonSerializer.Serialize(obj, type, (writeIndent ? jsonSerializeOptionsIndentedOnly : jsonSerializeOptions));
                return r;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        public static string ToJson(this object? obj, bool writeIndent = false)
        {
            if (obj == null) return string.Empty;
            try
            {
                return JsonSerializer.Serialize(obj, (writeIndent ? jsonSerializeOptionsIndentedOnly : jsonSerializeOptions));
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static byte[] ToJsonUtf8(Object obj)
        {
            try
            {
                return JsonSerializer.SerializeToUtf8Bytes(obj, jsonSerializeOptions);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static T? FromJson<T>(string obj) where T : class
        {
            try
            {
                var s = obj.Replace("\"Result\":", $"\"{typeof(T).Name}\":");
                //var r = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(s);
                if (typeof(T) == typeof(string))
                    return obj as T;
                var r = JsonSerializer.Deserialize<T>(s, jsonSerializeOptions);
                return r;
            }
            catch (Exception) { return null; }            
        }
        public static dynamic? FromJson(string? obj, Type type)
        {
            if (string.IsNullOrEmpty(obj))
                return null;
            else if (type == typeof(string))
                return obj;
            try
            {
                var r = JsonSerializer.Deserialize(obj, type, jsonSerializeOptions);
                var r2 = Convert.ChangeType(r, type);
                return r2;
            }
            catch (Exception){ return null; }
            //return r;
        }
        public static dynamic? FromJson(string? obj, string? assemblyQualifiedName)
        {
            try
            {
                if (string.IsNullOrEmpty(obj) || string.IsNullOrEmpty(assemblyQualifiedName)) return null;
                var type = Type.GetType(assemblyQualifiedName);
                if (type == null) return null;
                if (type == typeof(string))
                    return obj;
                var r = JsonSerializer.Deserialize(obj, type, jsonSerializeOptions);
                var r2 = Convert.ChangeType(r, type);
                return r2;
            }
            catch (Exception) { return null; }
            //return r;
        }

        public static T? FromJson<T>(byte[] bytes) where T : class
        {
            T? t = null;
            try
            {
                if (bytes == null || bytes.Length == 0) return null;
                //using (var stream = new MemoryStream(bytes))
                //{
                //    t = (T)JsonSerializer.DeserializeAsync(stream, typeof(T), jsonSerializeOptions).Result;
                //}
                var readOnlySpan = new ReadOnlySpan<byte>(bytes);
                var utf8Reader = new Utf8JsonReader(readOnlySpan);
                var r = JsonSerializer.Deserialize<T>(readOnlySpan, jsonSerializeOptions);
                t = r;

            }
            catch (Exception)
            {
                string s;
                try
                {
                    s = Encoding.UTF8.GetString(bytes);
                    //var r = JsonSerializer.Deserialize<T> (s, jsonSerializeOptions);
                    if (t != null && t is EventBaseClass ev)
                        ev.Message(s);
                }
                catch (Exception e2)
                { 
                    s = e2.Message;
                    if (t != null && t is EventBaseClass ev)
                        ev.Message(s);
                }
            }
            return t;
        }
        public static dynamic? FromJson(byte[] obj, Type type)
        {
            try
            {
                var utf8Reader = new Utf8JsonReader(obj);
                var r = JsonSerializer.Deserialize(ref utf8Reader, type, jsonSerializeOptions);
                return r;
            }
            catch (Exception) { return null; }
        }

        public static dynamic? FromJson(byte[] obj, string assemblyQualifiedName)
        {
            try
            {
                var type = Type.GetType(assemblyQualifiedName);
                if (type == null) return null;
                return FromJson(obj, type);
            }
            catch (Exception) { return null; }
        }


        private static JsonSerializerOptions jsonSerializeOptions = new()
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        //public static JsonSerializerOptions JsonSerializeOptions { get => jsonSerializeOptions; set => jsonSerializeOptions = value; }

        //private static JsonSerializerOptions jsonSerializeOptionsIndented = new()
        //{
        //    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
        //    WriteIndented = true
        //};

        private static JsonSerializerOptions jsonSerializeOptionsIndentedOnly = new() { WriteIndented = true };
        #endregion
    }
}
