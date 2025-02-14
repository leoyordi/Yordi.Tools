using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Yordi.Tools
{
    public enum TipoGuid
    {
        /// <summary>
        /// Ordem alfabética
        /// </summary>
        MySQL,
        /// <summary>
        /// Ordem binária
        /// </summary>
        Oracle,
        /// <summary>
        /// Ordem esquisita, baseada nos últimos 6 bytes (12 posições)
        /// </summary>
        MSSQL
    }
    /// <summary>
    /// Cria um novo GUID sequencial baseado no timestamp e tipo deeeeee banco de dados.
    /// Cada banco de dados tem uma forma diferente de ordenar GUIDs sequenciais.
    /// Esse método otimiza a ordenação dentro do banco de dados.
    /// </summary>
    public static class NewGuid
    {
        private static TipoGuid _seq = TipoGuid.MSSQL;
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public static TipoGuid TipoGuid { set { _seq = value; } }

        public static Guid NewSequentialGuid()
        {
            byte[] randomBytes = new byte[10];
            _rng.GetBytes(randomBytes);

            long timestamp = DateTime.UtcNow.Ticks / 10000L;
            byte[] timestampBytes = BitConverter.GetBytes(timestamp);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(timestampBytes);
            }

            byte[] guidBytes = new byte[16];

            switch (_seq)
            {
                case TipoGuid.MySQL:
                case TipoGuid.Oracle:
                    Buffer.BlockCopy(timestampBytes, 2, guidBytes, 0, 6);
                    Buffer.BlockCopy(randomBytes, 0, guidBytes, 6, 10);

                    // If formatting as a string, we have to reverse the order
                    // of the Data1 and Data2 blocks on little-endian systems.
                    if (_seq == TipoGuid.MySQL && BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(guidBytes, 0, 4);
                        Array.Reverse(guidBytes, 4, 2);
                    }
                    break;

                case TipoGuid.MSSQL:
                    Buffer.BlockCopy(randomBytes, 0, guidBytes, 0, 10);
                    Buffer.BlockCopy(timestampBytes, 2, guidBytes, 10, 6);
                    break;
            }

            return new Guid(guidBytes);
        }
    }
}
