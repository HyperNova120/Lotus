using System.Net;
using System.Net.Sockets;
using LotusCore.EngineEventArgs;
using LotusCore.Modules.LotusNetty.Packets;
using LotusCore.Utils;
using LotusCore.Utils.NBTInternals.Tags;
using static LotusCore.Modules.LotusNetty.Networking;

namespace LotusCore.Modules.LotusNetty.Internals
{
    public class ServerConnection
    {
        public Socket? _tcpSocket;

        public Guid _id;

        public ConnectionState _connectionState;

        public Encryption _encryption;

        public MinecraftPacketHandler _minecraftPacketHandler;

        public ServerConnectionSocketAsyncEventArgs _serverConnectionSocketAsyncEventArgs;

        public ConnectionInfo _connectionInfo;

        public PacketInfo _packetInfo;

        public ServerListInfo _serverListInfo;

        public ServerConnection(string serverIp, int port, Guid id)
        {
            _packetInfo = new();
            _serverListInfo = new();
            _connectionInfo = new();
            _id = id;
            _connectionInfo._remoteHost = IPAddress.Parse(serverIp);
            _connectionInfo._remotePort = port;
            _tcpSocket = null;
            _connectionState = ConnectionState.NONE;
            _encryption = new();
            _minecraftPacketHandler = new();
            _serverConnectionSocketAsyncEventArgs = new(_id);
        }
    }

    public struct ConnectionInfo
    {
        public IPAddress _remoteHost { get; set; }
        public int _remotePort { get; set; }
    }

    public class PacketInfo
    {
        public List<byte> _incompletePacketBytesBuffer = [];
        public List<byte> _dataToSendBuffer = new();

        public bool _activeBundleDelimiter = false;

        public Queue<MinecraftServerPacket> _bundledPackets = new();
    }

    public class ServerListInfo
    {
        public DateTime _LastPingTime;

        public double _LastPingLength;

        public TAG_Compound? _ServerListEntry;
    }
}
