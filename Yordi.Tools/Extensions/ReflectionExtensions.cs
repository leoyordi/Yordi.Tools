using System.Collections;
using System.Reflection;

namespace Yordi.Tools.Extensions
{
    public static class ReflectionExtensions
    {

        public static object? InvokeWithNamedParameters(this MethodBase self, object obj, List<Parametros>? parametros = null)
        {
            if (parametros == null || !parametros.Any(m => m.Valor != null))
                return self.Invoke(obj, null);
            if (parametros.Any(m => !string.IsNullOrEmpty(m.PropertyName)))
                return self.Invoke(obj, MapParametersByName(self, parametros));
            return self.Invoke(obj, MapParametersByType(self, parametros));
        }

        /// <summary>
        /// https://stackoverflow.com/questions/18455779/methodinfo-getparameter-with-nullable-type
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static bool MethodDontNeedParameter(this MethodBase method)
        {
            var parameters = method.GetParameters();
            if (parameters == null || parameters.Length == 0)
                return true;
            var defaultArgs = parameters.Where(a => a.HasDefaultValue || Nullable.GetUnderlyingType(a.ParameterType) != null);
            if (defaultArgs == null || defaultArgs.Count() == parameters.Length) return true;
            return false;
            //var isNull = parameters.Any(m => Nullable.GetUnderlyingType(m.ParameterType) != null);

        }

        public static object[]? MapParametersByType(MethodBase method, List<Parametros> namedParameters)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != namedParameters.Count)
                return null;
            List<object> lista = new List<object>();
            try
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    var item = namedParameters[i];
                    if (string.IsNullOrEmpty(item.AssemblyQualifiedName)) return null;
                    var type = Type.GetType(item.AssemblyQualifiedName);
                    if (type == null)
                        return null; // deu erro em conversão, não tem como aproveitar os parâmetros
                    bool isList = false;
                    var isAssignable = parameters[i].ParameterType.IsAssignableFrom(type);
                    if (!isAssignable)
                    {
                        if (!item.AssemblyQualifiedName.Contains("System.Xml") && !Equals(item.AssemblyQualifiedName, typeof(string).AssemblyQualifiedName))
                            isList = typeof(IEnumerable).IsAssignableFrom(type) && typeof(IEnumerable).IsAssignableFrom(parameters[i].ParameterType);
                    }
                    if (isAssignable || isList || string.Equals(item.AssemblyQualifiedName, parameters[i].ParameterType.AssemblyQualifiedName))
                    {
                        var conversao = Conversores.FromJson(item.Valor, type);
                        lista.Add(conversao);
                    }
                    else
                        return null; // Mapeamento não confere, retorna null, ignorando anteriores
                }
            }
            catch { return null; }
            if (lista.Count == 0) return null;
            return lista.ToArray();
        }

        public static object[]? MapParametersByName(MethodBase method, List<Parametros> namedParameters)
        {
            //pegando os parâmetros do método indicado
            var parameters = method.GetParameters();
            if (parameters.Length != namedParameters.Count)
                return null;

            //criar uma nova lista de parâmetros para associar o que foi requisitado com o que há no método
            // e carregar um valor inicial (Type.Missing)
            List<object> lista = new List<object>();
            try
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    var named = namedParameters.FirstOrDefault(m => string.Equals(m.PropertyName, parameters[i].Name));
                    if (named != null && !string.IsNullOrEmpty(named.AssemblyQualifiedName) && !string.IsNullOrEmpty(named.Valor))
                    {
                        var type = Type.GetType(named.AssemblyQualifiedName);
                        if (type == null)
                            return null; // deu erro em conversão, não tem como aproveitar os parâmetros
                        var conversao = Conversores.FromJson(named.Valor, type);
                        lista.Add(conversao);
                    }
                    else
                        return null; // parâmetro não encontrado;
                }
            }
            catch { return null; }
            if (lista.Count == 0) return null;
            return lista.ToArray();
        }

        public static (bool, object?) Mapped(Parametros item, ParameterInfo info)
        {
            if (string.IsNullOrEmpty(item.AssemblyQualifiedName))
                return (false, null);
            var type = Type.GetType(item.AssemblyQualifiedName);
            if (type == null)
                return (false, null); // deu erro em conversão, não tem como aproveitar os parâmetros
            bool isList = false;
            var isAssignable = info.ParameterType.IsAssignableFrom(type);
            if (!isAssignable)
            {
                if (!item.AssemblyQualifiedName.Contains("System.Xml") && !Equals(item.AssemblyQualifiedName, typeof(string).AssemblyQualifiedName))
                    isList = typeof(IEnumerable).IsAssignableFrom(type) && typeof(IEnumerable).IsAssignableFrom(info.ParameterType);
            }
            if (isAssignable || isList || string.Equals(item.AssemblyQualifiedName, info.ParameterType.AssemblyQualifiedName))
            {
                var conversao = Conversores.FromJson(item.Valor, type);
                return (true, conversao);
            }
            else
                return (false, null); // Mapeamento não confere, retorna null, ignorando anteriores
        }
    }
    public class Parametros
    {
        public string? AssemblyQualifiedName { get; set; }

        /// <summary>
        /// Qualquer objeto deverá ser convertido em string pelo JsonSerializer
        /// </summary>
        public string? Valor { get; set; }

        public string? PropertyName { get; set; }
    }
}
