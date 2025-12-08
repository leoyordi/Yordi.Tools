using System;
using System.Reflection;

namespace Yordi.Tools
{

    public class CommonColumns : ICommonColumns
    {
        /// <summary>
        /// Atualização automática de Data só funciona no MySQL
        /// </summary>
        [AutoInsertDate, OnlyInsert]
        public virtual DateTime? DataInclusao { get; set; }

        /// <summary>
        /// Atualização automática de Data só funciona no MySQL
        /// </summary>
        [AutoInsertDate, AutoUpdateDate, OnlyUpdate]
        public virtual DateTime? DataAlteracao { get; set; }

        [OnlyInsert]
        public virtual string? UsuarioInclusao { get; set; }
        [OnlyUpdate]
        public virtual string? UsuarioAlteracao { get; set; }
        public virtual string? Origem { get; set; }

    }


    public class Basico : CommonColumns, IAuto, IDescricao, IObjectStringIndexer, IPropertyType
    {
        private readonly Type myType;
        public Basico()
        {
            myType = GetType();
        }
        [AutoIncrement]
        public virtual int Auto { get; set; }
        public virtual string? Descricao { get; set; }

        [BDIgnorar]
        public static string DisplayMember { get { return "Descricao"; } }
        [BDIgnorar]
        public static string ValueMember { get { return "Auto"; } }

        [BDIgnorar, System.Text.Json.Serialization.JsonIgnore]
        public virtual object? this[string propertyName]
        {
            get
            {
                // probably faster without reflection:
                // like:  return Properties.Settings.Default.PropertyValues[propertyName] 
                // instead of the following
                PropertyInfo? myPropInfo = myType.GetProperty(propertyName);
                if (myPropInfo == null)
                    return null;
                return myPropInfo.GetValue(this, null);
            }
            set
            {
                PropertyInfo? myPropInfo = myType.GetProperty(propertyName);
                if (myPropInfo == null)
                    return;
                Type t = Nullable.GetUnderlyingType(myPropInfo.PropertyType) ?? myPropInfo.PropertyType;
                object? safeValue = value == null ? null : Convert.ChangeType(value, t);
                myPropInfo.SetValue(this, safeValue, null);
            }
        }
        public virtual Type? GetPropertyType(string propertyName)
        {
            PropertyInfo? myPropInfo = myType.GetProperty(propertyName);
            if (myPropInfo == null)
                return null;
            return myPropInfo.PropertyType;
        }

        public override string ToString()
        {
            return $"[{Auto}] {Descricao}";
        }
    }

    /// <summary>
    /// Interface para classes POCO que precisam definir índices de banco de dados.
    /// Permite que entidades especifiquem quais índices devem ser criados e verificados no banco de dados.
    /// </summary>
    public interface IPOCOIndexes
    {
        /// <summary>
        /// Retorna a lista de índices que devem ser criados para esta entidade no banco de dados.
        /// </summary>
        /// <returns>Uma coleção de IndexInfo com as definições dos índices, ou null/vazio se não houver índices.</returns>
        IEnumerable<IndexInfo> GetIndexes();

        /// <summary>
        /// Representa a definição de um índice de banco de dados.
        /// </summary>
        public class IndexInfo
        {
            /// <summary>
            /// Nome do índice no banco de dados.
            /// </summary>
            public string IndexName { get; set; } = string.Empty;
            
            /// <summary>
            /// Lista de colunas que compõem o índice.
            /// </summary>
            public List<string> Columns { get; set; } = new List<string>();
            
            /// <summary>
            /// Indica se o índice é único (UNIQUE INDEX).
            /// </summary>
            public bool IsUnique { get; set; } = false;
            
            /// <summary>
            /// Coleção de chaves (campos) para compor o índice quando necessário.<br/>
            /// Se o índice tiver cláusula WHERE, carregar as informações de cada parâmetro do WHERE aqui.
            /// </summary>
            public IEnumerable<Chave> Chaves { get; set; } = Enumerable.Empty<Chave>();
        }
    }

}
