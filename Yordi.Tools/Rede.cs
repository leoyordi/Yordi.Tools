using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace Yordi.Tools
{
    public class Rede
    {
        public static async Task<string?> IP()
        {
            string ip = string.Empty;
            try
            {
                string url = "http://checkip.dyndns.org";
                //System.Net.WebRequest req = System.Net.WebRequest.Create(url);
                //System.Net.WebResponse resp = req.GetResponse();
                //System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                //string response = sr.ReadToEnd().Trim();
                string response = await new System.Net.Http.HttpClient().GetStringAsync(url);
                string[] ipAddressWithText = response.Split(':');
                string ipAddressWithHTMLEnd = ipAddressWithText[1].Substring(1);
                string[] ipAddress = ipAddressWithHTMLEnd.Split('<');
                ip = ipAddress[0];
            }
            catch (Exception ex)
            {
                Logger.LogSync(ex);
                try
                {
                    var host = System.Net.Dns.GetHostEntry(Environment.MachineName);
                    if (host?.AddressList == null || host.AddressList.Length == 0) return null;
                    foreach (System.Net.IPAddress IP in host.AddressList)
                    {
                        if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            ip = Convert.ToString(IP) ?? string.Empty;
                    }
                }
                catch (Exception exII)
                {
                    Logger.LogSync(exII);
                }
            }
            //Message($"Meu IP com o exterior: {ip}");
            return ip;
        }

        public static string? MeuMACAddress(IPAddress ip)
        {
            string ipString = ip.ToString();
            NetworkInterface[]? nics = NetworkInterface
                                        .GetAllNetworkInterfaces()
                                        ?.Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                                        ?.ToArray();
            if (nics == null || nics.Length == 0) return null;
            foreach (NetworkInterface adapter in nics)
            {
                foreach (UnicastIPAddressInformation unip in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (ipString.Contains(unip.Address.ToString()))
                    {
                        PhysicalAddress address = adapter.GetPhysicalAddress();
                        return BitConverter.ToString(address.GetAddressBytes());
                    }
                }
            }
            return null;
        }


#pragma warning disable CA1416 // Validar a compatibilidade da plataforma
        public static IEnumerable<string>? MeuMacAddressForWindows()
        {
            StringBuilder s = new StringBuilder();
            s.Append("SELECT MACAddress, PNPDeviceID ");
            s.Append("FROM Win32_NetworkAdapter ");
            s.Append("WHERE MACAddress IS NOT NULL AND PNPDeviceID IS NOT NULL");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher();
            searcher.Query = new ObjectQuery(s.ToString());
            ManagementObjectCollection mObject = searcher.Get();
            List<string> macs = new List<string>();
            foreach (ManagementObject obj in mObject)
            {
                string? pnp = obj["PNPDeviceID"]?.ToString();
                if (!string.IsNullOrEmpty(pnp) && pnp.Contains("PCI\\"))
                {
                    string? mac = obj["MACAddress"]?.ToString();
                    if (string.IsNullOrEmpty(mac)) continue;
                    macs.Add(mac);
                }
            }
            if (macs == null || macs.Count == 0) return null;
            return macs;
        }
        
        public static IEnumerable<string>? MeuMacAddressForWindows(IPAddress ip)
        {
            string? mac = MeuMACAddress(ip);
            if (!string.IsNullOrEmpty(mac)) return new List<string>() { mac };
            StringBuilder s = new StringBuilder();
            s.Append("SELECT MACAddress, PNPDeviceID ");
            s.Append("FROM Win32_NetworkAdapter ");
            s.Append("WHERE MACAddress IS NOT NULL AND PNPDeviceID IS NOT NULL");

            ManagementObjectSearcher searcher = new();
            searcher.Query = new ObjectQuery(s.ToString());
            ManagementObjectCollection mObject = searcher.Get();
            List<string> macs = new List<string>();
            foreach (ManagementObject obj in mObject)
            {
                string? pnp = obj["PNPDeviceID"].ToString();
                if (!string.IsNullOrEmpty(pnp) && pnp.Contains("PCI\\"))
                {
                    mac = obj["MACAddress"]?.ToString();
                    if (string.IsNullOrEmpty(mac)) continue;
                    macs.Add(mac);
                }
            }
            if (macs == null || macs.Count == 0) return null;
            return macs;
        }
#pragma warning restore CA1416 // Validar a compatibilidade da plataforma

        public static IPAddress? GetIP(string host)
        {
            IPHostEntry ipHostInfo;
            if (string.IsNullOrEmpty(host))
            {
                ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            }
            else if (ValidaObjetos.IsIP(host, out IPAddress? address))
            {
                return address;
            }
            else
            {
                var split = host.Split(':');
                if (split == null)
                    ipHostInfo = Dns.GetHostEntry(host);
                else
                    ipHostInfo = Dns.GetHostEntry(split[0]);
            }
            if ((ipHostInfo != null) && (ipHostInfo.AddressList.Length > 0))
            {
                // check for the first address not null
                // it seems that with .Net Micro Framework, the IPV6 addresses aren't supported and return "null"
                int i = 0;
                while (ipHostInfo.AddressList[i] == null) i++;
                return ipHostInfo.AddressList[i];
            }
            return null;
        }

    }
}
