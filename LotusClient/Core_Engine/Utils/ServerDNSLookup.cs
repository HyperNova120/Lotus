using System.Net;
using System.Threading.Tasks;
using DnsClient;
using Silk.NET.OpenGL;

namespace LotusCore.Utils;

public static class ServerDNSLookup
{
    private static LookupClient lookup = new LookupClient();

    private static string? DNSGetHostAddresses(string dnsQuery)
    {
        try
        {
            return Dns.GetHostAddresses(dnsQuery)[0].ToString();
        }
        catch
        {
            return null;
        }
    }

    private static async Task<string?> DNSGetHostAddressesAsync(string dnsQuery)
    {
        try
        {
            return (await Dns.GetHostAddressesAsync(dnsQuery))[0].ToString();
        }
        catch
        {
            return null;
        }
    }

    public static (string? serverIP, int? port) GetServerDNSRecord(string dnsQuery)
    {
        if (string.IsNullOrWhiteSpace(dnsQuery))
        {
            return (null, null);
        }

        var parts = dnsQuery.Split(":");
        if (parts.Length == 2)
        {
            //if dnsQuery has specified port to use
            if (!int.TryParse(parts[1], out int PortResult))
            {
                Logging.LogDebug($"GetServerDNSRecord: PortResult Fail");
                PortResult = 25565;
            }
            string? remoteHostIP;
            remoteHostIP = DNSGetHostAddresses(parts[0]);
            return (remoteHostIP, (remoteHostIP != null) ? PortResult : null);
        }

        //if dnsQuery has no specified port
        var result = lookup.Query($"_minecraft._tcp.{dnsQuery}", QueryType.SRV);
        var srvRecord = result.Answers.SrvRecords().FirstOrDefault();
        string? dnsResult;
        if (srvRecord != null)
        {
            dnsResult = DNSGetHostAddresses(srvRecord.Target.Value.TrimEnd('.'));
            return (dnsResult, (dnsResult != null) ? srvRecord.Port : null);
        }
        dnsResult = DNSGetHostAddresses(dnsQuery);
        return (dnsResult, (dnsResult != null) ? 25565 : null);
    }

    public static async Task<(string? serverIP, int? port)> GetServerDNSRecordAsync(string dnsQuery)
    {
        if (string.IsNullOrWhiteSpace(dnsQuery))
        {
            return (null, null);
        }

        var parts = dnsQuery.Split(":");
        if (parts.Length == 2)
        {
            //if dnsQuery has specified port to use
            if (!int.TryParse(parts[1], out int PortResult))
            {
                Logging.LogDebug($"GetServerDNSRecord: PortResult Fail");
                PortResult = 25565;
            }
            string? remoteHostIP;
            remoteHostIP = await DNSGetHostAddressesAsync(parts[0]);
            return (remoteHostIP, (remoteHostIP != null) ? PortResult : null);
        }

        //if dnsQuery has no specified port
        var result = await lookup.QueryAsync($"_minecraft._tcp.{dnsQuery}", QueryType.SRV);
        var srvRecord = result.Answers.SrvRecords().FirstOrDefault();
        string? dnsResult;
        if (srvRecord != null)
        {
            dnsResult = await DNSGetHostAddressesAsync(srvRecord.Target.Value.TrimEnd('.'));
            return (dnsResult, (dnsResult != null) ? srvRecord.Port : null);
        }
        dnsResult = await DNSGetHostAddressesAsync(dnsQuery);
        return (dnsResult, (dnsResult != null) ? 25565 : null);
    }
}
