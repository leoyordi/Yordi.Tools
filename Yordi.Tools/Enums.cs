using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace Yordi.Tools
{
    public interface IAuto { int Auto { get; set; } }
    public interface IDescricao { string? Descricao { get; set; } }
    public interface IClone<T> { T Clone(); }
    public interface IObjectStringIndexer { object? this[string propertyName] { get; set; } }
    public interface IObservacao { string Observacao { get; set; } }
    public interface IToString { string ToString(); }
    public interface IPropertyType
    {
        Type? GetPropertyType(string propertyName);
    }

    public interface ICommonColumns
    {
        DateTime? DataAlteracao { get; set; }
        DateTime? DataInclusao { get; set; }
        string? Origem { get; set; }
        string? UsuarioAlteracao { get; set; }
        string? UsuarioInclusao { get; set; }
    }
    public interface IDisplayValueMember
    {
        string DisplayMember { get; }
        string ValueMember { get; }
    }

    /// <summary>
    /// Operadores de comparação usados para montar instruções SQL, especialmente as relacionadas à clausula WHERE.<br/>
    /// Por exemplo: CONTÉM, COMECAcom e TERMINAcom são usados para montar instruções SQL do tipo LIKE e acrescenta o coringa % onde cabe.
    /// </summary>
    public enum Operador
    {
        IGUAL,
        MAIORque,
        MENORque,
        MAIORouIGUALque,
        MENORouIGUALque,
        CONTÉM,
        COMECAcom,
        TERMINAcom
    }
    public enum SimNao
    {
        NÃO,
        SIM
    }
    public enum TipoDB
    {
        [Description("Nenhum")]
        NONE = 0,
        [Description("SQL Server")]
        SQLServer = 1,
        [Description("MySQL")]
        MySQL = 4,
        [Description("SQLite")]
        SQLite = 5
    }

    /// <summary>
    /// Tipos de dados suportados, usados para criar as instruções SQL num formato respeitado por cada tipo de banco de dados.<br/>
    /// Por exemplo: BOOL será convertido em tinyint(1) no MySQL e bit no SQL Server.
    /// </summary>
    public enum Tipo
    {
        STRING,
        INT,
        DOUBLE,
        MONEY,
        DATA,
        HORA,
        BOOL,
        GUID,
        ENUM,
        BLOB
    }


    public class EnumDisplay: IDescricao
    {
        public int Valor { get; set; }
        public string? Descricao { get; set; }
        [BDIgnorar]
        public static string DisplayMember { get { return "Descricao"; } }
        [BDIgnorar]
        public static string ValueMember { get { return "Valor"; } }

    }

    public static class EnumExtensions
    {
        public static string GetEnumDescription<T>(T enumeratedType)
        {
            if (enumeratedType == null) return string.Empty;
            string? description = enumeratedType.ToString();
            if (string.IsNullOrEmpty(description)) return string.Empty;
            var enumType = typeof(T);
            // Can't use type constraints on value types, so have to do check like this
            //if (!enumType.IsEnum)
            //    throw new ArgumentException("Variável precisa ser do tipo Enum");
            var fieldInfo = enumeratedType.GetType().GetField(description);
            if (fieldInfo != null)
            {
                var attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes != null && attributes.Length > 0)
                    description = ((DescriptionAttribute)attributes[0]).Description;
            }
            return description;
        }

        public static string GetEnumCollectionDescription<T>(Collection<T> enums)
        {
            var sb = new StringBuilder();

            var enumType = typeof(T);

            // Can't use type constraints on value types, so have to do check like this
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("Variável precisa ser do tipo Enum");

            foreach (var enumeratedType in enums)
                sb.AppendLine(GetEnumDescription<T>(enumeratedType));

            return sb.ToString();
        }
        public static string GetDescription<T>(this T enumeratedType)
        {
            if (enumeratedType == null) return string.Empty;
            string? description = enumeratedType.ToString();
            if (string.IsNullOrEmpty(description)) return string.Empty;
            if (enumeratedType.GetType().BaseType != typeof(Enum))
                throw new ArgumentException("Variável precisa ser do tipo Enum");
            var fieldInfo = enumeratedType.GetType().GetField(description);
            if (fieldInfo != null)
            {
                var attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes != null && attributes.Length > 0)
                    description = ((DescriptionAttribute)attributes[0]).Description;
            }
            return description;
        }

        public static IList<EnumDisplay> ToDisplay<T>()
        {            
            return Enum.GetValues(typeof(T))
                        .Cast<Enum>()
                        .Select(value => {

                            return new EnumDisplay()
                            {
                                Descricao = value.GetDescription(),
                                Valor = Convert.ToInt32(value)
                            };
                        })
                        .OrderBy(item => item.Valor)
                        .ToList();
        }

        public static T GetValueFromString<T>(string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (string.Equals(field.Name, description, StringComparison.CurrentCultureIgnoreCase))
                {
                    var valor = field.GetValue(null);
                    if (valor != null)
                        return (T)valor;
                }
            }
            throw new ArgumentException("Not found.", nameof(description));
        }

    }
}
