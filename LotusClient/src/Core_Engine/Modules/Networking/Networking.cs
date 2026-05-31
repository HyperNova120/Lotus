using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.LotusNetty.Internals;
using LotusCore.Modules.LotusNetty.Packets;
using LotusCore.Modules.LotusNetty.Types;
using LotusCore.Modules.ServerConfig;
using LotusCore.Modules.ServerLogin;
using LotusCore.Modules.ServerPlay;
using static LotusCore.Modules.LotusNetty.Internals.ProtocolVersionUtils;

namespace LotusCore.Modules.LotusNetty
{
    public class Networking : INetworkModule, IModuleBase
    {
        private ConnectionHandler? _connectionHandler;

        public readonly ProtocolVersionUtils.ProtocolVersion _protocolVersion = ProtocolVersionUtils
            .ProtocolVersion
            .V1_21_10;

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

        public void RegisterEvents(Action<string> RegisterEvent)
        {
            RegisterEvent.Invoke("STATUS_Packet_Received");
            RegisterEvent.Invoke("LOGIN_Packet_Received");
            RegisterEvent.Invoke("CONFIG_Packet_Received");
            RegisterEvent.Invoke("PLUGIN_Packet_Received");
            RegisterEvent.Invoke("PLAY_Packet_Received");
        }

        public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent) { }

        public void LinkModules(ICoreModule coreModule)
        {
            _connectionHandler = new(coreModule);
            _connectionHandler._loginPacketHandler = coreModule.GetModule<LoginHandler>()!;
            _connectionHandler._configPacketHandler = coreModule.GetModule<ServerConfiguration>()!;
            _connectionHandler._playPacketHandler =
                coreModule.GetModule<IServerPlayHandlerModule>()!;
            _connectionHandler._statusPacketHandler =
                coreModule.GetModule<ServerList.ServerList>()!;
        }

        public void LoginSuccessful(Guid remoteHostID)
        {
            GetServerConnection(remoteHostID)!._connectionState = ConnectionState.CONFIGURATION;
            SetIsClientConnectedToPrimaryServer(true);
            _connectionHandler!.SetPrimaryConnection(remoteHostID);
        }

        public int SendPacket(
            Guid RemoteHostID,
            MinecraftPacket packet,
            bool HoldPacketInBuffer = false
        )
        {
            ServerConnection? connection = GetServerConnection(RemoteHostID);
            if (connection == null)
            {
                Logging.LogError("Attempting to send packet to null connection");
                DisconnectFromServer(RemoteHostID);
                return -1;
            }

            if (connection._tcpSocket == null)
            {
                return -1;
            }
            connection._packetInfo._dataToSendBuffer.AddRange(
                connection._minecraftPacketHandler.CreatePacket(connection, packet)
            );

            if (HoldPacketInBuffer)
            {
                return 0;
            }

            return SendBufferedPackets(connection);
        }

        public int SendPackets(
            Guid RemoteHostId,
            IEnumerable<MinecraftPacket> packets,
            bool HoldPacketInBuffer = false
        )
        {
            ServerConnection? connection = GetServerConnection(RemoteHostId);
            if (connection == null)
            {
                Logging.LogError("Attempting to send packet to null connection");
                DisconnectFromServer(RemoteHostId);
                return -1;
            }

            if (connection._tcpSocket == null)
            {
                return -1;
            }

            foreach (MinecraftPacket minecraftPacket in packets)
            {
                connection._packetInfo._dataToSendBuffer.AddRange(
                    connection._minecraftPacketHandler.CreatePacket(connection, minecraftPacket)
                );
            }

            if (HoldPacketInBuffer)
            {
                return 0;
            }

            return SendBufferedPackets(connection);
        }

        public int SendBufferedPackets(Guid RemoteHostId)
        {
            return SendBufferedPackets(GetServerConnection(RemoteHostId));
        }

        public int SendBufferedPackets(ServerConnection? connection)
        {
            if (connection == null)
            {
                Logging.LogError("Attempting to send packet to null connection");
                return -1;
            }

            if (connection._tcpSocket == null)
            {
                return -1;
            }

            int bytesSent = 0;

            while (bytesSent < connection._packetInfo._dataToSendBuffer.Count)
            {
                bytesSent += connection._tcpSocket.Send(
                    connection._packetInfo._dataToSendBuffer.ToArray(),
                    bytesSent,
                    connection._packetInfo._dataToSendBuffer.Count - bytesSent,
                    SocketFlags.None
                );
            }
            connection._packetInfo._dataToSendBuffer.Clear();
            return bytesSent;
        }

        public Guid? ConnectToServer(string ip, int port = 25565)
        {
            return _connectionHandler!.ConnectToServer(ip, port);
        }

        public void DisconnectFromServer(Guid remoteHostID)
        {
            _connectionHandler!.DisconnectFromServer(remoteHostID);
        }

        public ServerConnection? GetServerConnection(Guid connectionID)
        {
            return _connectionHandler!.GetServerConnection(connectionID);
        }

        public ProtocolVersion GetProtocolVersion()
        {
            return _protocolVersion;
        }

        public bool IsClientConnectedToPrimaryServer()
        {
            return _connectionHandler!.IsClientConnectedToPrimaryServer();
        }

        public void SetIsClientConnectedToPrimaryServer(bool value)
        {
            _connectionHandler!._isClientConnectedToPrimaryServer = value;
        }

        public Guid? GetServerConnectionInState(
            IPAddress connectionID,
            IEnumerable<ConnectionState> connectionStates
        )
        {
            return _connectionHandler!.GetConnectionInState(connectionID, connectionStates);
        }
    }

    public enum ConnectionState
    {
        STATUS,
        LOGIN,
        CONFIGURATION,
        PLAY,
        NONE,
    }
}
