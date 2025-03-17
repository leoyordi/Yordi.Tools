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
}
