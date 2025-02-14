using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Yordi.Tools
{
    public static class ValidaObjetos
    {
        public static bool IsEmail(string email)
        {
            if (String.IsNullOrEmpty(email))
                return false;
            if (email.Length < 8)
                return false;

            //const string patternOLD = @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))" +
            //  @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$";

            const String pattern =
               @"^([0-9a-zA-Z]" + //Start with a digit or alphabetical
               @"([\+\-_\.][0-9a-zA-Z]+)*" + // No continuous or ending +-_. chars in email
               @")+" +
               @"@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,17})$";

            if (!Regex.IsMatch(email, pattern))
                return false;
            else
                return true;
        }
        public static bool IsCPF(string cpf)
        {
            if (String.IsNullOrEmpty(cpf))
                return false;

            string cpfLimpo;
            cpfLimpo = cpf.Trim();
            cpfLimpo = cpfLimpo.Replace(".", "").Replace("-", "");
            if (cpfLimpo.Length != 11)
                return false;

            if (!ulong.TryParse(cpfLimpo, out ulong numero))
                return false;

            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;
            tempCpf = cpfLimpo.Substring(0, 9);
            soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = digito + resto.ToString();
            return cpfLimpo.EndsWith(digito);
        }
        public static bool IsCNPJ(string cnpj)
        {
            if (String.IsNullOrEmpty(cnpj))
                return false;

            string cnpjLimpo = cnpj.Trim().Replace(".", "").Replace("-", "").Replace("/", "");
            if (cnpjLimpo.Length != 14)
                return false;

            if (!ulong.TryParse(cnpjLimpo, out ulong n))
                return false;

            int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma;
            int resto;
            string digito;
            string tempCnpj;
            tempCnpj = cnpjLimpo.Substring(0, 12);
            soma = 0;
            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];
            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCnpj = tempCnpj + digito;
            soma = 0;
            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];
            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = digito + resto.ToString();
            return cnpjLimpo.EndsWith(digito);
        }
        public static bool IsFone(string fone)
        {
            {
                /*
                    [RegularExpression(@"^\(?[1-9][0-9]\)?[. -]?[9]{0,1}[2-9]{1}[0-9]{3}[. -]?[0-9]{4}$",
                    ErrorMessage ="Formatos: (XX) ?XXXX-XXXX ou XX ?XXXX XXXX ou XX-?XXXX-XXXX")]
                */
                ///^[(]?[0-9]{2}[)]?[0-9]{4,5}[-. ]?[0-9]{4}$/im
                string MatchPhoneNumberPattern = "^\\(?[1-9][0-9]\\)?[. -]?[9]{0,1}[2-9]{1}[0-9]{3}[. -]?[0-9]{4}$";
                if (!String.IsNullOrEmpty(fone))
                    return Regex.IsMatch(fone, MatchPhoneNumberPattern);
                else
                    return false;
            }
        }
        public static bool IsCircuito(string circuito)
        {
            /*
                [RegularExpression(@"^\(?[1-9][0-9]\)?[. -]?[9]{0,1}[2-9]{1}[0-9]{3}[. -]?[0-9]{4}$",
                ErrorMessage ="Formatos: (XX) ?XXXX-XXXX ou XX ?XXXX XXXX ou XX-?XXXX-XXXX")]
            */
            ///^[(]?[0-9]{2}[)]?[0-9]{4,5}[-. ]?[0-9]{4}$/im
            string MatchPhoneNumberPattern = "^?[A-Z][A-Z]?[ -]?[0-9]{3}$";
            if (!String.IsNullOrEmpty(circuito))
                return Regex.IsMatch(circuito, MatchPhoneNumberPattern);
            else
                return false;
        }

        public static bool IsMacAddress(string macAddress)
        {
            bool result;
            Regex rxMacAddress;

            if (String.IsNullOrEmpty(macAddress) || (macAddress.Length < 12) || (macAddress.Length > 17))
            {
                result = false;
            }
            else if (macAddress.Length == 12)
            {
                rxMacAddress = new Regex(@"^[0-9a-fA-F]{12}$");
                result = rxMacAddress.IsMatch(macAddress);
            }
            else
            {
                rxMacAddress = new Regex(@"^([0-9A-F]{2}[:-]){5}([0-9A-F]{2})$");
                result = rxMacAddress.IsMatch(macAddress);
            }

            return result;
        }

        public static bool IsIPv4(string value)
        {
            var quads = value.Split('.');

            // if we do not have 4 quads, return false
            if (!(quads.Length == 4)) return false;

            // for each quad
            foreach (var quad in quads)
            {
                int q;
                // if parse fails 
                // or length of parsed int != length of quad string (i.e.; '1' vs '001')
                // or parsed int < 0
                // or parsed int > 255
                // return false
                if (!Int32.TryParse(quad, out q)
                    || !q.ToString().Length.Equals(quad.Length)
                    || q < 0
                    || q > 255) { return false; }

            }

            return true;
        }

        public static bool IsIP(string value, out IPAddress? address)
        {
            return IPAddress.TryParse(value, out address);
        }
        public static bool IsURL(string value)
        {
            return Uri.TryCreate(value, UriKind.Absolute, out _ /*var uri*/); // && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
        public static bool IsIPorURL(string value)
        {
            return IsIP(value, out _) || IsURL(value);
        }
        public static bool IsInt(string value, out int valor)
        {
            return int.TryParse(value, out valor);
        }
        public static bool IsDouble(string value, out double valor)
        {
            return double.TryParse(value, out valor);
        }

        // Sequential version
        // assumes sequential enum members 0,1,2,3,4 -etc.            
        // 
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static bool IsEnumValid(Enum enumValue, int value, int minValue, int maxValue)
        {
            bool valid = (value >= minValue) && (value <= maxValue);
            return valid;
        }


        #region Testando tipo genérico
        /// <summary>
        /// https://stackoverflow.com/questions/982487/testing-if-object-is-of-generic-type-in-c-sharp#answer-29823390
        /// </summary>
        /// <param name="typeToCheck"></param>
        /// <param name="genericType"></param>
        /// <returns></returns>
        public static bool IsOfGenericType(this Type typeToCheck, Type genericType, [CallerArgumentExpression("genericType")] string? paramName = default)
        {
            Type? concreteType;
            try
            {
                return typeToCheck.IsOfGenericType(genericType, out concreteType, paramName);
            }
            catch(Exception ex)
            {
                Logger.LogSync(ex.Message);
                return false;
            }
        }

        public static bool IsOfGenericType(this Type typeToCheck, Type genericType, out Type? concreteGenericType, [CallerArgumentExpression("genericType")] string? paramName = default)
        {
            if (genericType.IsAssignableFrom(typeToCheck))
            {
                concreteGenericType = typeToCheck;
                return true;
            }

            while (true)
            {
                concreteGenericType = null;

                if (genericType == null)
                    throw new ArgumentNullException(nameof(genericType), "Tipo da comparação é nulo aqui");

                if (!genericType.IsGenericTypeDefinition)
                    throw new ArgumentException($"O tipo da comparação precisa ser também um GenericTypeDefinition. Tipo identificado: {genericType.Name}", paramName);

                if (typeToCheck == null || typeToCheck == typeof(object))
                    return false;

                if (typeToCheck == genericType)
                {
                    concreteGenericType = typeToCheck;
                    return true;
                }

                if ((typeToCheck.IsGenericType ? typeToCheck.GetGenericTypeDefinition() : typeToCheck) == genericType)
                {
                    concreteGenericType = typeToCheck;
                    return true;
                }

                if (genericType.IsInterface)
                    foreach (var i in typeToCheck.GetInterfaces())
                        if (i.IsOfGenericType(genericType, out concreteGenericType, paramName))
                            return true;
                if (typeToCheck.BaseType == null)
                    return false;
                typeToCheck = typeToCheck.BaseType;
            }
        }

        #endregion
    }
    public static class Compara
    {
        public static bool AreEqual(this string a, string b)
        {
            if (String.IsNullOrEmpty(a))
                return String.IsNullOrEmpty(b);
            else if (String.IsNullOrEmpty(b))
                return false;
            else
                return String.Equals(a, b);
        }
        public static bool AreEqual(this double? a, double? b)
        {
            if (!a.HasValue)
                return b.HasValue;
            else if (!b.HasValue)
                return false;
            else
                return a.Value == b.Value;
        }

        public static bool AreEqual(byte[] b1, byte[] b2)
        {
            if (b1 == b2) return true;
            if (b1 == null || b2 == null) return false;
            if (b1.Length != b2.Length) return false;
            return b1.SequenceEqual(b2);
        }
    }

}
