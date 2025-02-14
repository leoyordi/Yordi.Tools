using System.Net.Sockets;

namespace Yordi.Tools.Extensions
{
    public static class SocketExtensions
    {
        private const int BytesPerLong = 4; // 32 / 8
        private const int BitsPerByte = 8;

        public static bool IsDisposed(this Socket socket)
        {
            try
            {
                if (socket == null)
                    return true;
                int av = socket.Available;
                return false;
            }
            catch (ObjectDisposedException) 
            {
                Logger.LogSync("Socket descartado");
                return true; 
            }
            catch (Exception ex) 
            { 
                EventBaseClass.GetLogger().LogError(ex); 
                return false; 
            }

        }
        public static bool IsSocketConnected(this Socket socket)
        {
            try
            {
                if (socket == null)
                    return false;
                if (!socket.Connected)
                    return false;
                if (socket.Poll(1, SelectMode.SelectError))
                    return false;
                if (socket.Poll(1, SelectMode.SelectRead))
                    return true;
                if(socket.Poll(1, SelectMode.SelectWrite))
                    return true;
                return false;
            }
            catch (SocketException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// Sets the keep-alive interval for the socket.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="time">Time between two keep alive "pings".</param>
        /// <param name="interval">Time between two keep alive "pings" when first one fails.</param>
        /// <returns>If the keep alive infos were succefully modified.</returns>
        public static bool SetKeepAlive(this Socket socket, ulong time, ulong interval)
        {
            try
            {
                // Array to hold input values.
                var input = new[]
                {
                    (time == 0 || interval == 0) ? 0UL : 1UL, // on or off
                    time,
                    interval
                };

                // Pack input into byte struct.
                byte[] inValue = new byte[3 * BytesPerLong];
                for (int i = 0; i < input.Length; i++)
                {
                    inValue[i * BytesPerLong + 3] = (byte)(input[i] >> ((BytesPerLong - 1) * BitsPerByte) & 0xff);
                    inValue[i * BytesPerLong + 2] = (byte)(input[i] >> ((BytesPerLong - 2) * BitsPerByte) & 0xff);
                    inValue[i * BytesPerLong + 1] = (byte)(input[i] >> ((BytesPerLong - 3) * BitsPerByte) & 0xff);
                    inValue[i * BytesPerLong + 0] = (byte)(input[i] >> ((BytesPerLong - 4) * BitsPerByte) & 0xff);
                }

                // Create bytestruct for result (bytes pending on server socket).
                byte[] outValue = BitConverter.GetBytes(0);

                // Write SIO_VALS to Socket IOControl.
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
#pragma warning disable CA1416 // Validar a compatibilidade da plataforma
                socket.IOControl(IOControlCode.KeepAliveValues, inValue, outValue);
#pragma warning restore CA1416 // Validar a compatibilidade da plataforma
            }
            catch (SocketException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Define configurações para o KeepAlive
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="keepAliveInterval">Intervalo entre 'pings', em segundos</param>
        /// <param name="retryInterval">Intervalo para tentar se reconectar, em segundos</param>
        /// <param name="maxRetryCount">Número de tentativas de reconexão</param>
        /// <returns></returns>
        public static bool SetKeepAlive(this Socket socket, uint keepAliveInterval, uint retryInterval, uint maxRetryCount)
        {
            try
            {
                uint alive = keepAliveInterval * 1000;
                uint retry = retryInterval * 1000;
                uint max = maxRetryCount;
                // Array to hold input values.
                var ioControlSet = new byte[]
                {
                    1, 0, 0, 0,
                    (byte)(alive & 0xff), (byte)((alive >> 8) & 0xff), (byte)((alive >> 16) & 0xff), (byte)((alive >> 24) & 0xff),
                    (byte)(retry & 0xff), (byte)((retry >> 8) & 0xff), (byte)((retry >> 16) & 0xff), (byte)((retry >> 24) & 0xff),
                    (byte)(max & 0xff), (byte)((max >> 8) & 0xff), (byte)((max >> 16) & 0xff), (byte)((max >> 24) & 0xff)
                };

                // Write SIO_VALS to Socket IOControl.
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
#pragma warning disable CA1416 // Validar a compatibilidade da plataforma
                socket.IOControl(IOControlCode.KeepAliveValues, ioControlSet, null);
#pragma warning restore CA1416 // Validar a compatibilidade da plataforma
            }
            catch (SocketException)
            {
                return false;
            }

            return true;
        }

    }
}
