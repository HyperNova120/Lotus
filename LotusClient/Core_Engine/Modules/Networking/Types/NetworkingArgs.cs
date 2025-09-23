using System.Data;
using System.Net;
using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Modules.Networking;
using LotusCore.Modules.Networking.Packets;
using Microsoft.AspNetCore.Authentication.OAuth;

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

    public IEnumerable<Networking.ConnectionState> _connectionStates;

    public GetServerConnectionInStateArgs(
        IPAddress remoteHost,
        IEnumerable<Networking.ConnectionState> connectionStates
    )
    {
        _remoteHost = remoteHost;
        _connectionStates = connectionStates;
    }
}
