using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Yordi.Tools
{
    public delegate Task PositionSizeDelegate(XYHL positionSize);

    /// <summary>
    /// Define a posição e tamanho de um controle em um formulário ou área
    /// </summary>
    public class XYHL : IEquatable<XYHL?>
    {
        private DateTime created;
        public XYHL()
        {
            created = DateTime.Now;
        }
        [Key]
        /// <summary>
        /// Nome do formulário que esse controle faz parte
        /// </summary>
        public string? Form { get; set; }

        [Key]
        /// <summary>
        /// Nome do controle
        /// </summary>
        public string? Nome { get; set; }

        /// <summary>
        /// Controle Pai, caso necessário
        /// </summary>
        public string? ParentControl { get; set; }

        /// <summary>
        /// Posição X do controle
        /// </summary>
        public decimal X { get; set; }

        /// <summary>
        /// Posição Y do controle
        /// </summary>
        public decimal Y { get; set; }

        /// <summary>
        /// Altura do controle (Height)
        /// </summary>
        public decimal H { get; set; }

        /// <summary>
        /// Largura do controle (Width)
        /// </summary>
        public decimal L { get; set; }

        [BDIgnorar, JsonIgnore]
        public Size Size => new Size((int)L, (int)H);
        [BDIgnorar, JsonIgnore]
        public Point Location => new Point((int)X, (int)Y);

        public override string ToString()
        {
            return Conversores.ToJson(this);
        }
        public bool Equals(XYHL? other)
        {
            if (other == null) return false;
            return Equals(Nome, other.Nome) && Equals(other.Form, Form);
        }

        public XYHL Clone()
        {
            return new XYHL()
            {
                Form = Form,
                H = H,
                Y = Y,
                L = L,
                Nome = Nome,
                ParentControl = ParentControl,
                X = X,
            };
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as XYHL);
        }

        public override int GetHashCode()
        {
            return created.GetHashCode();
        }
    }
}
