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
            Logging.LogDebug($"DNS RESULT NULL");
            return (null, null);
        }

        var parts = dnsQuery.Split(":");
        if (parts.Length == 2)
        {
            //if dnsQuery has specified port to use
            if (!int.TryParse(parts[1], out int PortResult))
            {
                //Logging.LogDebug($"GetServerDNSRecord: PortResult Fail");
                PortResult = 25565;
            }
            string? remoteHostIP;
            remoteHostIP = DNSGetHostAddresses(parts[0]);

            Logging.LogDebug(
                $"DNS RESULT 1 for dnsQuery:{dnsQuery}: remoteHostIP:{remoteHostIP} PORT:{PortResult}"
            );
            return (remoteHostIP, (remoteHostIP != null) ? PortResult : null);
        }

        //if dnsQuery has no specified port
        //Logging.LogDebug($"GetServerDNSRecord: _minecraft._tcp.{dnsQuery}");
        var result = lookup.Query($"_minecraft._tcp.{dnsQuery}", QueryType.SRV);
        var srvRecord = result.Answers.SrvRecords().FirstOrDefault();
        string? dnsResult;
        if (srvRecord != null)
        {
            dnsResult = DNSGetHostAddresses(srvRecord.Target.Value.TrimEnd('.'));
            Logging.LogDebug($"DNS RESULT 2 for dnsQuery:{dnsQuery}: srvRecord:{srvRecord.Port}");
            return (dnsResult, (dnsResult != null) ? srvRecord.Port : null);
        }
        dnsResult = DNSGetHostAddresses(dnsQuery);
        Logging.LogDebug($"DNS RESULT 3 for dnsQuery:{dnsQuery}: dnsResult:{dnsResult}");
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
                //Logging.LogDebug($"GetServerDNSRecord: PortResult Fail");
                PortResult = 25565;
            }
            string? remoteHostIP;
            remoteHostIP = await DNSGetHostAddressesAsync(parts[0]);
            // Logging.LogDebug($"GetServerDNSRecord: {dnsQuery}, remoteHostIP: {remoteHostIP}");
            return (remoteHostIP, (remoteHostIP != null) ? PortResult : null);
        }

        //if dnsQuery has no specified port
        //Logging.LogDebug($"GetServerDNSRecord: _minecraft._tcp.{dnsQuery}");
        var result = await lookup.QueryAsync($"_minecraft._tcp.{dnsQuery}", QueryType.SRV);
        var srvRecord = result.Answers.SrvRecords().FirstOrDefault();
        string? dnsResult;
        if (srvRecord != null)
        {
            dnsResult = await DNSGetHostAddressesAsync(srvRecord.Target.Value.TrimEnd('.'));
            /* Logging.LogDebug(
                $"GetServerDNSRecord: SRV _minecraft._tcp.{dnsQuery}, dnsResult: {dnsResult}"
            ); */
            return (dnsResult, (dnsResult != null) ? srvRecord.Port : null);
        }
        dnsResult = await DNSGetHostAddressesAsync(dnsQuery);
        /* Logging.LogDebug(
            $"GetServerDNSRecord: NO SRV _minecraft._tcp.{dnsQuery}, dnsResult: {dnsResult}"
        ); */
        return (dnsResult, (dnsResult != null) ? 25565 : null);
    }
}
