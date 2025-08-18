using System.Net;
using System.Net.Sockets;
using Core_Engine.EngineEventArgs;
using static Core_Engine.Modules.Networking.Networking;

namespace Core_Engine.Modules.Networking.Internals
{
    public class ServerConnection
    {
        public Socket? _TcpSocket;
        public ConnectionState _ConnectionState;
        public Encryption _Encryption;
        public MinecraftPacketHandler _MinecraftPacketHandler;

        public ServerConnectionSocketAsyncEventArgs _ServerConnectionSocketAsyncEventArgs;

        public IPAddress _RemoteHost { get; private set; }

        public byte[] _IncompletePacketBytesBuffer = [];
        public List<byte> _DataToSendBuffer = new();

        public ServerConnection(string serverAddress)
        {
            _RemoteHost = Dns.GetHostAddresses(serverAddress)[0];
            _TcpSocket = null;
            _ConnectionState = ConnectionState.NONE;
            _Encryption = new();
            _MinecraftPacketHandler = new();
            _ServerConnectionSocketAsyncEventArgs = new(_RemoteHost);
        }
    }
}
