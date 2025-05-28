using System.Net;
using System.Net.Sockets;
using Core_Engine.EngineEventArgs;
using static Core_Engine.Modules.Networking.Networking;

namespace Core_Engine.Modules.Networking.Internals
{
    public class ServerConnection
    {
        public Socket? TcpSocket;
        public ConnectionState connectionState;
        public Encryption encryption;
        public MinecraftPacketHandler minecraftPacketHandler;

        public ServerConnectionSocketAsyncEventArgs serverConnectionSocketAsyncEventArgs;

        public IPAddress remoteHost { get; private set; }

        public ServerConnection(string serverAddress)
        {
            remoteHost = Dns.GetHostAddresses(serverAddress)[0];
            TcpSocket = null;
            connectionState = ConnectionState.NONE;
            encryption = new();
            minecraftPacketHandler = new();
            serverConnectionSocketAsyncEventArgs = new(remoteHost);
        }
    }
}
