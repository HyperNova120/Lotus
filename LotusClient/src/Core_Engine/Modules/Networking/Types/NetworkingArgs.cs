using System.Net;
using LotusCore.EngineEventArgs;
using LotusCore.Modules.LotusNetty;
using LotusCore.Modules.LotusNetty.Packets;

public class SendPacketArgs : IEngineEventArgs
{
    public Guid _remoteHostID;

    public MinecraftPacket _packet;

    public bool _holdPacketInBuffer;

    public SendPacketArgs(
        Guid remoteHostID,
        MinecraftPacket packet,
        bool holdPacketInBuffer = false
    )
    {
        _remoteHostID = remoteHostID;
        _packet = packet;
        _holdPacketInBuffer = holdPacketInBuffer;
    }
}

public class SendPacketsArgs : IEngineEventArgs
{
    public Guid _remoteHostID;

    public IEnumerable<MinecraftPacket> _packets;

    public bool _holdPacketInBuffer;

    public SendPacketsArgs(
        Guid remoteHostID,
        IEnumerable<MinecraftPacket> packet,
        bool holdPacketInBuffer = false
    )
    {
        _remoteHostID = remoteHostID;
        _packets = packet;
        _holdPacketInBuffer = holdPacketInBuffer;
    }
}

public class ConnectToServerArgs : IEngineEventArgs
{
    public string _ip;

    public int _port;

    public ConnectToServerArgs(string ip, int port)
    {
        _ip = ip;
        _port = port;
    }
}

public class GetServerConnectionInStateArgs : IEngineEventArgs
{
    public IPAddress _remoteHost;

    public IEnumerable<ConnectionState> _connectionStates;

    public GetServerConnectionInStateArgs(
        IPAddress remoteHost,
        IEnumerable<ConnectionState> connectionStates
    )
    {
        _remoteHost = remoteHost;
        _connectionStates = connectionStates;
    }
}
