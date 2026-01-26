using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Yordi.Tools
{
    /// <summary>
    /// Informações detalhadas de um endereço IP
    /// </summary>
    public class InfoRede
    {
        public IPAddress IP { get; set; } = IPAddress.None;
        public string IPString => IP.ToString();
        public TipoRede Tipo { get; set; }
        public string NomeAdaptador { get; set; } = string.Empty;
        public string DescricaoAdaptador { get; set; } = string.Empty;
        public IPAddress? MascaraSubRede { get; set; }
        public IPAddress? Gateway { get; set; }
        public string? TipoVPN { get; set; }
        public NetworkInterfaceType TipoInterface { get; set; }
        public OperationalStatus Status { get; set; }

        public override string ToString()
        {
            var tipoDesc = Tipo == TipoRede.VPN && !string.IsNullOrEmpty(TipoVPN)
                ? $"VPN ({TipoVPN})"
                : Tipo.ToString();
            return $"IP {IPString} | {tipoDesc} | {NomeAdaptador}";
        }

        /// <summary>
        /// Verifica se dois IPs estão na mesma sub-rede
        /// </summary>
        public bool EstaNaMesmaRede(IPAddress outroIP)
        {
            if (MascaraSubRede == null || IP.AddressFamily != outroIP.AddressFamily)
                return false;

            var ipBytes = IP.GetAddressBytes();
            var outroBytes = outroIP.GetAddressBytes();
            var mascaraBytes = MascaraSubRede.GetAddressBytes();

            for (int i = 0; i < ipBytes.Length; i++)
            {
                if ((ipBytes[i] & mascaraBytes[i]) != (outroBytes[i] & mascaraBytes[i]))
                    return false;
            }
            return true;
        }
    }

    public class Rede
    {
        private static readonly string[] VpnKeywords =
        {
            "VPN", "TAP", "TUN", "WireGuard", "Fortinet", "Cisco",
            "GlobalProtect", "OpenVPN", "NordVPN", "ExpressVPN",
            "Surfshark", "ProtonVPN", "Pulse Secure", "AnyConnect"
        };

        /// <summary>
        /// Obtém informações completas de todas as interfaces de rede ativas
        /// </summary>
        public static List<InfoRede> GetAllNetworkInfo()
        {
            var resultado = new List<InfoRede>();

            var nics = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up);

            foreach (var nic in nics)
            {
                var ipProps = nic.GetIPProperties();
                var gateway = ipProps.GatewayAddresses.FirstOrDefault()?.Address;

                foreach (var unicast in ipProps.UnicastAddresses)
                {
                    if (unicast.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    var info = new InfoRede
                    {
                        IP = unicast.Address,
                        NomeAdaptador = nic.Name,
                        DescricaoAdaptador = nic.Description,
                        MascaraSubRede = unicast.IPv4Mask,
                        Gateway = gateway,
                        TipoInterface = nic.NetworkInterfaceType,
                        Status = nic.OperationalStatus,
                        Tipo = ClassificarIP(unicast.Address, nic, out string? tipoVpn),
                        TipoVPN = tipoVpn
                    };

                    resultado.Add(info);
                }
            }

            return resultado;
        }

        /// <summary>
        /// Classifica um IP baseado no endereço e características do adaptador
        /// </summary>
        private static TipoRede ClassificarIP(IPAddress ip, NetworkInterface nic, out string? tipoVpn)
        {
            tipoVpn = null;
            var bytes = ip.GetAddressBytes();

            // Loopback
            if (IPAddress.IsLoopback(ip) || bytes[0] == 127)
                return TipoRede.Loopback;

            // Link-local (APIPA)
            if (bytes[0] == 169 && bytes[1] == 254)
                return TipoRede.LinkLocal;

            // Verifica se é VPN
            tipoVpn = DetectarTipoVPN(nic);
            if (tipoVpn != null)
                return TipoRede.VPN;

            // Rede privada (RFC 1918)
            if (IsRedePrivada(bytes))
                return TipoRede.RedeLocal;

            // Se chegou aqui e não é privado, é público
            return TipoRede.Web;
        }

        /// <summary>
        /// Verifica se é um endereço de rede privada (RFC 1918)
        /// </summary>
        private static bool IsRedePrivada(byte[] bytes)
        {
            // 10.0.0.0 - 10.255.255.255
            if (bytes[0] == 10)
                return true;

            // 172.16.0.0 - 172.31.255.255
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                return true;

            // 192.168.0.0 - 192.168.255.255
            if (bytes[0] == 192 && bytes[1] == 168)
                return true;

            return false;
        }

        /// <summary>
        /// Detecta o tipo de VPN baseado no adaptador de rede
        /// </summary>
        private static string? DetectarTipoVPN(NetworkInterface nic)
        {
            // Tipos de interface que indicam VPN
            if (nic.NetworkInterfaceType == NetworkInterfaceType.Ppp)
                return "PPP";
            if (nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                return "Tunnel";

            // Busca por palavras-chave na descrição
            foreach (var keyword in VpnKeywords)
            {
                if (nic.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    nic.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return keyword;
                }
            }

            return null;
        }

        /// <summary>
        /// Obtém o IP externo (público) da máquina
        /// </summary>
        public static async Task<InfoRede?> GetIPWebAsync()
        {
            string[] services =
            {
                "https://api.ipify.org",
                "https://icanhazip.com",
                "https://checkip.amazonaws.com",
                "http://checkip.dyndns.org"
            };

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

            foreach (var url in services)
            {
                try
                {
                    string response = await client.GetStringAsync(url);

                    // checkip.dyndns.org retorna HTML
                    if (url.Contains("dyndns"))
                    {
                        var parts = response.Split(':');
                        if (parts.Length > 1)
                        {
                            response = parts[1].Split('<')[0].Trim();
                        }
                    }

                    response = response.Trim();

                    if (IPAddress.TryParse(response, out var ip))
                    {
                        return new InfoRede
                        {
                            IP = ip,
                            Tipo = TipoRede.Web,
                            NomeAdaptador = "Internet",
                            DescricaoAdaptador = $"IP público obtido via {new Uri(url).Host}"
                        };
                    }
                }
                catch
                {
                    continue;
                }
            }

            return null;
        }

        /// <summary>
        /// Obtém IPs filtrados por tipo de rede
        /// </summary>
        public static List<InfoRede> GetIPsByTipo(TipoRede tipo)
        {
            return GetAllNetworkInfo().Where(i => i.Tipo == tipo).ToList();
        }

        /// <summary>
        /// Obtém o melhor IP para publicar um serviço, considerando a localização do cliente
        /// </summary>
        /// <param name="ipCliente">IP do cliente que vai se conectar (opcional)</param>
        /// <returns>O IP mais apropriado para a conexão</returns>
        public static async Task<InfoRede?> GetBestIPForServiceAsync(IPAddress? ipCliente = null)
        {
            var todosIPs = GetAllNetworkInfo()
                .Where(i => i.Tipo != TipoRede.Loopback && i.Tipo != TipoRede.LinkLocal)
                .ToList();

            // Se tem IP do cliente, verifica se estão na mesma rede
            if (ipCliente != null)
            {
                // Primeiro verifica VPNs (prioridade se cliente está na VPN)
                var vpnMatch = todosIPs
                    .Where(i => i.Tipo == TipoRede.VPN)
                    .FirstOrDefault(i => i.EstaNaMesmaRede(ipCliente));

                if (vpnMatch != null)
                    return vpnMatch;

                // Depois verifica rede local
                var localMatch = todosIPs
                    .Where(i => i.Tipo == TipoRede.RedeLocal)
                    .FirstOrDefault(i => i.EstaNaMesmaRede(ipCliente));

                if (localMatch != null)
                    return localMatch;
            }

            // Se não encontrou rede comum, retorna IP público
            var ipWeb = await GetIPWebAsync();
            if (ipWeb != null)
                return ipWeb;

            // Fallback: retorna primeiro IP de rede local disponível
            return todosIPs.FirstOrDefault(i => i.Tipo == TipoRede.RedeLocal)
                ?? todosIPs.FirstOrDefault();
        }

        /// <summary>
        /// Obtém todos os IPs disponíveis para publicação de serviço (exclui loopback e link-local)
        /// </summary>
        public static async Task<List<InfoRede>> GetAllPublishableIPsAsync()
        {
            var resultado = GetAllNetworkInfo()
                .Where(i => i.Tipo != TipoRede.Loopback && i.Tipo != TipoRede.LinkLocal)
                .ToList();

            var ipWeb = await GetIPWebAsync();
            if (ipWeb != null)
                resultado.Add(ipWeb);

            return resultado;
        }

        /// <summary>
        /// Resume todas as informações de rede em formato texto
        /// </summary>
        public static async Task<string> GetNetworkSummaryAsync()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Informações de Rede ===");
            sb.AppendLine();

            var infos = await GetAllPublishableIPsAsync();
            var grouped = infos.GroupBy(i => i.Tipo);

            foreach (var group in grouped.OrderBy(g => g.Key))
            {
                sb.AppendLine($"[{group.Key}]");
                foreach (var info in group)
                {
                    sb.AppendLine($"  • {info}");
                    if (info.Gateway != null)
                        sb.AppendLine($"    Gateway: {info.Gateway}");
                    if (info.MascaraSubRede != null)
                        sb.AppendLine($"    Máscara: {info.MascaraSubRede}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Obtém informações do IP local da máquina
        /// </summary>
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

        /// <summary>
        /// Obtém o MAC address do adaptador de rede correspondente a um determinado IP
        /// </summary>
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
        /// <summary>
        /// Obtém todos os MAC addresses dos adaptadores de rede no Windows
        /// </summary>
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

        /// <summary>
        /// Obtém o MAC address do adaptador de rede correspondente a um determinado IP no Windows
        /// </summary>
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

        /// <summary>
        /// Obtém o endereço IP correspondente a um nome de host
        /// </summary>
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