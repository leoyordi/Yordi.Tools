using System.ComponentModel;
using System.Reflection;

namespace System
{
    public class APIAttribute : Attribute { }
    public class AutoIncrementAttribute : Attribute { }
    public class BDIgnorarAttribute : Attribute { }
    public class ExibirAttribute : Attribute { }
    public class ValorAttribute : Attribute { }
    public class AutoUpdateDate : Attribute { }
    public class AutoInsertDate : Attribute { }
    public class OnlyInsert : Attribute { }
    public class OnlyUpdate : Attribute { }

    /// <summary>
    /// Usar com os parênteses se a instrução SQL exigir
    /// </summary>
    public class TamanhoAttribute : Attribute
    {
        protected string _tamanho;
        /// <summary>
        /// Usar com os parênteses se a instrução SQL exigir
        /// </summary>
        /// <param name="tamanho">Exemplos: "(2048)", "(18,4)"</param>
        public TamanhoAttribute(string tamanho)
        {
            _tamanho = tamanho;
        }
        public string Tamanho { get { return _tamanho; } }
    }

    public class StringNameAttribute : Attribute
    {
        protected string _name;
        public StringNameAttribute(string tamanho)
        {
            _name = tamanho;
        }
        public string Nameof { get { return _name; } }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class POCOtoDBAttribute : Attribute
    {
        public POCOType Tipo { get; set; }
        public POCOtoDBAttribute() { }
        public POCOtoDBAttribute(POCOType tipo)
        {
            Tipo = tipo;
        }
    }
    public enum POCOType
    {
        NONE = 0,
        CADASTRO = 1,
        MOVIMENTO = 2,
        CONFIG = 3,
        TUDO = 4,
    }

    /// <summary>
    /// Identificar o DisplayMember e o ValueMember da classe para controles como ListBox e Combobox
    /// Os atributos que identificam são [Exibir] e [Valor]
    /// </summary>
    /// <typeparam name="T">Classe ou Entidade</typeparam>
    public static class ExibirCampo<T> where T : class
    {
        private static BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
        private static T obj = Activator.CreateInstance<T>();

        /// <summary>
        /// Identificar o DisplayMember da classe para controles como ListBox e Combobox
        /// </summary>
        /// <typeparam name="T">Classe ou Entidade</typeparam>
        public static String Display
        {
            get
            {
                Type type = obj.GetType();
                PropertyInfo[] properties = type.GetProperties(flags);

                foreach (PropertyInfo p in properties)
                {
                    var att = Attribute.GetCustomAttribute(p, typeof(ExibirAttribute)) as ExibirAttribute;
                    if (att != null)
                        return p.Name;

                }
                return obj.GetType().Name;
            }
        }

        /// <summary>
        /// Identificar o ValueMember da classe para controles como ListBox e Combobox
        /// </summary>
        /// <typeparam name="T">Classe ou Entidade</typeparam>
        public static String Valor
        {
            get
            {
                Type type = obj.GetType();
                PropertyInfo[] properties = type.GetProperties(flags);

                foreach (PropertyInfo p in properties)
                {
                    var att = Attribute.GetCustomAttribute(p, typeof(ValorAttribute)) as ValorAttribute;
                    if (att != null)
                        return p.Name;

                }
                return obj.GetType().Name;
            }
        }


    }

    public static class GetMethodsHelper
    {
        /// <summary>
        /// Traz uma lista de métodos com que contenha o atributo informado
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetMethodsWithAttribute(this Type classType, Type attributeType)
        {
            return classType.GetMethods().Where(methodInfo => methodInfo.GetCustomAttributes(attributeType, true).Length > 0);
        }

        /// <summary>
        /// Traz uma lista de propriedades com que contenha o atributo informado
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public static IEnumerable<MemberInfo> GetMembersWithAttribute(this Type classType, Type attributeType)
        {
            return classType.GetMembers().Where(memberInfo => memberInfo.GetCustomAttributes(attributeType, true).Length > 0);
        }

    }


    public static class AttributeTools
    {
        public static object? GetDefaultValue(this PropertyInfo property)
        {
            var defaultAttr = property.GetCustomAttribute(typeof(DefaultValueAttribute));
            if (defaultAttr != null && defaultAttr is DefaultValueAttribute vAtt)
                return vAtt.Value;
            //defaultAttr = property.GetCustomAttribute(typeof(ValorPadraoAttribute));
            //if (defaultAttr != null)
            //    return (defaultAttr as ValorPadraoAttribute).ValorPadrao;
            var propertyType = property.PropertyType;
            propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            return propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null;
        }
    }
}
