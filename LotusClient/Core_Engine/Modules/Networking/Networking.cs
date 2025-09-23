using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.Networking.Internals;
using LotusCore.Modules.Networking.Packets;
using LotusCore.Modules.Networking.Types;

namespace LotusCore.Modules.Networking
{
    public class Networking : IModuleBase
    {
        private Dictionary<Guid, ServerConnection> _connections = new();

        public bool _isClientConnectedToPrimaryServer { get; set; } = false;

        private Guid? _primaryClientServerConnection;

        public readonly ProtocolVersionUtils.ProtocolVersion _protocolVersion = ProtocolVersionUtils
            .ProtocolVersion
            .V1_21_8;

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

        public void RegisterEvents(Action<string> RegisterEvent)
        {
            RegisterEvent.Invoke("STATUS_Packet_Received");
            RegisterEvent.Invoke("LOGIN_Packet_Received");
            RegisterEvent.Invoke("CONFIG_Packet_Received");
            RegisterEvent.Invoke("PLUGIN_Packet_Received");
            RegisterEvent.Invoke("PLAY_Packet_Received");

            RegisterEvent.Invoke("NETWORKING_Version");
            RegisterEvent.Invoke("NETWORKING_SendPacket");
            RegisterEvent.Invoke("NETWORKING_SendPackets");
            RegisterEvent.Invoke("NETWORKING_SendBufferedPackets");
            RegisterEvent.Invoke("NETWORKING_ConnectToServer");
            RegisterEvent.Invoke("NETWORKING_DisconnectFromServer");
            RegisterEvent.Invoke("NETWORKING_GetServerConnection");
            RegisterEvent.Invoke("NETWORKING_GetServerConnectionInState");
            RegisterEvent.Invoke("NETWORKING_GetIsClientConnectedToPrimaryServer");
            RegisterEvent.Invoke("NETWORKING_SetIsClientConnectedToPrimaryServer");
        }

        public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent)
        {
            SubscribeToEvent.Invoke(
                "SERVERLOGIN_loginSuccessful",
                new EngineEventHandler(
                    (sender, args) =>
                    {
                        var connArgs = (ConnectionEventArgs)args!;
                        GetServerConnection(connArgs._remoteHostID)!._connectionState =
                            ConnectionState.CONFIGURATION;
                        _primaryClientServerConnection = connArgs._remoteHostID;
                        _isClientConnectedToPrimaryServer = true;
                        return null;
                    }
                )
            );
            SubscribeToEvent.Invoke(
                "NETWORKING_Version",
                new EngineEventHandler(
                    (sender, _) =>
                    {
                        return new ProtocolVersionResult(_protocolVersion);
                    }
                )
            );
            SubscribeToEvent.Invoke(
                "NETWORKING_SendPacket",
                new EngineEventHandler(
                    (sender, args) =>
                    {
                        SendPacketArgs Args = (SendPacketArgs)args!;
                        return new IntResult(
                            SendPacket(Args._remoteHostID, Args._packet, Args._holdPacketInBuffer)
                        );
                    }
                )
            );
            SubscribeToEvent.Invoke(
                "NETWORKING_SendPackets",
                new EngineEventHandler(
                    (sender, args) =>
                    {
                        SendPacketsArgs Args = (SendPacketsArgs)args!;
                        return new IntResult(
                            SendPackets(Args._remoteHostID, Args._packets, Args._holdPacketInBuffer)
                        );
                    }
                )
            );
            SubscribeToEvent.Invoke(
                "NETWORKING_SendBufferedPackets",
                new EngineEventHandler(
                    (sender, args) =>
                    {
                        GuidEngineArgs Args = (GuidEngineArgs)args!;
                        return new IntResult(SendBufferedPackets(Args._value));
                    }
                )
            );
            SubscribeToEvent.Invoke(
                "NETWORKING_ConnectToServer",
                new EngineEventHandler(
                    (sender, args) =>
                    {
                        ConnectToServerArgs Args = (ConnectToServerArgs)args!;
                        return new GuidResult(ConnectToServer(Args._ip, Args._port));
                    }
                )
            );
            SubscribeToEvent.Invoke(
                "NETWORKING_DisconnectFromServer",
                new EngineEventHandler(
                    (sender, args) =>
                    {
                        GuidEngineArgs Args = (GuidEngineArgs)args!;
                        DisconnectFromServer(Args._value);
                        return null;
                    }
                )
            );
            SubscribeToEvent.Invoke(
                "NETWORKING_GetServerConnection",
                new EngineEventHandler(
                    (sender, args) =>
                    {
                        GuidEngineArgs Args = (GuidEngineArgs)args!;
                        return new ServerConnectionResult(GetServerConnection(Args._value));
                    }
                )
            );
            SubscribeToEvent.Invoke(
                "NETWORKING_GetServerConnectionInState",
                new EngineEventHandler(
                    (sender, args) =>
                    {
                        GetServerConnectionInStateArgs Args = (GetServerConnectionInStateArgs)args!;
                        foreach (var con in _connections.Values)
                        {
                            if (
                                con._connectionInfo._remoteHost == Args._remoteHost
                                && Args._connectionStates.Contains(con._connectionState)
                            )
                            {
                                return new GuidResult(con._id);
                            }
                        }
                        return new GuidResult(null);
                    }
                )
            );
            SubscribeToEvent.Invoke(
                "NETWORKING_GetIsClientConnectedToPrimaryServer",
                new EngineEventHandler(
                    (_, _) =>
                    {
                        return new BoolResult(_isClientConnectedToPrimaryServer);
                    }
                )
            );
            SubscribeToEvent.Invoke(
                "NETWORKING_SetIsClientConnectedToPrimaryServer",
                new EngineEventHandler(
                    (_, args) =>
                    {
                        BoolEngineArgs Args = (BoolEngineArgs)args!;
                        _isClientConnectedToPrimaryServer = Args._value;
                        return null;
                    }
                )
            );
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
            Guid id = Guid.CreateVersion7();
            ServerConnection serverConnection = new(ip, port, id);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            serverConnection._tcpSocket = new Socket(
                endPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
            );
            serverConnection._tcpSocket.NoDelay = true;
            try
            {
                serverConnection._tcpSocket.Connect(endPoint);
            }
            catch (Exception e)
            {
                return null;
            }
            _connections[serverConnection._id] = serverConnection;

            ResetBuffer(serverConnection._serverConnectionSocketAsyncEventArgs);
            serverConnection._serverConnectionSocketAsyncEventArgs.Completed += ReceiveCompleted;

            StartReceiving(serverConnection._serverConnectionSocketAsyncEventArgs);
            Logging.LogDebug($"Successfully Connected to Server: {ip}:{port}");
            return id;
        }

        public void DisconnectFromServer(Guid remoteHostID)
        {
            ServerConnection? connection = GetServerConnection(remoteHostID);
            if (connection == null)
            {
                return;
            }
            Logging.LogInfo("Disconnected from Server:" + remoteHostID);
            if (
                _isClientConnectedToPrimaryServer
                && connection._id == _primaryClientServerConnection
            )
            {
                _isClientConnectedToPrimaryServer = false;
                _primaryClientServerConnection = null;
            }
            if (connection!._tcpSocket != null)
            {
                connection!._tcpSocket!.Disconnect(false);
                connection!._tcpSocket!.Close();
                connection!._tcpSocket = null;
            }
            _connections.Remove(remoteHostID);
            return;
        }

        public ServerConnection? GetServerConnection(Guid connectionID)
        {
            if (_connections.ContainsKey(connectionID))
            {
                return _connections[connectionID];
            }
            return null;
        }

        private void StartReceiving(SocketAsyncEventArgs e)
        {
            ServerConnectionSocketAsyncEventArgs eventArgs =
                (ServerConnectionSocketAsyncEventArgs)e;
            ServerConnection? connection = GetServerConnection(eventArgs._remoteHostID);
            if (connection == null)
            {
                Logging.LogError("Server Connection Null");
                return;
            }
            if (!connection._tcpSocket!.ReceiveAsync(e))
            {
                ReceiveCompleted(this, e);
            }
        }

        private void ResetBuffer(SocketAsyncEventArgs e)
        {
            byte[] receivedBuffer = new byte[0x3FFFFF];
            e.SetBuffer(receivedBuffer, 0, receivedBuffer.Length);
        }

        private void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            //Logging.LogDebug("ReceiveCompleted");
            ServerConnectionSocketAsyncEventArgs eventArgs =
                (ServerConnectionSocketAsyncEventArgs)e;
            ServerConnection connection = GetServerConnection(eventArgs._remoteHostID)!;
            try
            {
                if (ProcessReceive(e))
                {
                    ResetBuffer(e);
                    if (connection._tcpSocket != null)
                    {
                        StartReceiving(e);
                    }
                }
                else
                {
                    Core_Engine.SignalInteractiveResetServerHolds();
                }
            }
            catch (Exception exc)
            {
                Logging.LogError($"Handle Packet Received ERROR: {exc}");
                DisconnectFromServer(connection._id);
                if (
                    _isClientConnectedToPrimaryServer
                    && _primaryClientServerConnection == eventArgs._remoteHostID
                )
                {
                    Core_Engine.SignalInteractiveResetServerHolds();
                }
            }
        }

        private bool ProcessReceive(SocketAsyncEventArgs e)
        {
            ServerConnectionSocketAsyncEventArgs eventArgs =
                (ServerConnectionSocketAsyncEventArgs)e;
            if (e.SocketError == SocketError.Success)
            {
                try
                {
                    ServerConnection connection = GetServerConnection(eventArgs._remoteHostID)!;
                    //data received properly
                    byte[] tmpBuffer = e.Buffer![..e.BytesTransferred];
                    /* Logging.LogDebug(
                        $"ProcessReceive {packetBytes.Length} Bytes received; State: {connectionState}"
                    ); */
                    if (tmpBuffer.Length == 0)
                    {
                        Logging.LogInfo("Connection Closed by Remote Host");
                        DisconnectFromServer(eventArgs._remoteHostID);
                        return false;
                    }
                    if (connection._minecraftPacketHandler._IsEncryptionEnabled)
                    {
                        //Logging.LogDebug("Decrypting Packet");
                        tmpBuffer = connection._encryption.DecryptData(tmpBuffer);
                    }

                    connection._packetInfo._incompletePacketBytesBuffer =
                    [
                        .. connection._packetInfo._incompletePacketBytesBuffer,
                        .. tmpBuffer,
                    ];

                    bool firstRun = true;
                    while (connection._packetInfo._incompletePacketBytesBuffer.Length > 0)
                    {
                        if (!firstRun)
                        {
                            Logging.LogDebug("\tMulti packet receive");
                        }
                        (
                            MinecraftServerPacket? serverPacket,
                            connection._packetInfo._incompletePacketBytesBuffer
                        ) = connection._minecraftPacketHandler.DecodePacket(
                            connection._id,
                            connection._packetInfo._incompletePacketBytesBuffer
                        );
                        if (serverPacket == null)
                        {
                            /* Logging.LogDebug(
                                $"ReceiveConnections; bad packet, Remaining Size:{connection._IncompletePacketBytesBuffer.Length}"
                            ); */
                            break;
                            /* Logging.LogDebug("ReceiveConnections; bad packet, cancelling");
                            DisconnectFromServer(eventArgs.remoteHost);
                            return false; */
                        }
                        ProcessPacketEvent(connection, serverPacket);
                    }
                    return true;
                }
                catch (Exception exc)
                {
                    Logging.LogError($"Process Received Packet Error: {exc}");
                    return false;
                }
            }
            Logging.LogError($"Socket Error: {e.SocketError}");
            return false;
        }

        private void ProcessPacketEvent(ServerConnection connection, MinecraftServerPacket packet)
        {
            /* Logging.LogDebug(
                $"\tProcessing Packet 0x{packet._Protocol_ID:X} in State: {connection._ConnectionState.ToString()}"
            ); */
            try
            {
                switch (connection._connectionState)
                {
                    case ConnectionState.STATUS:
                        Core_Engine.InvokeEvent(
                            "STATUS_Packet_Received",
                            new PacketReceivedEventArgs(packet, connection._id)
                        );
                        break;
                    case ConnectionState.LOGIN:
                        Core_Engine.InvokeEvent(
                            "LOGIN_Packet_Received",
                            new PacketReceivedEventArgs(packet, connection._id)
                        );
                        break;
                    case ConnectionState.CONFIGURATION:
                        Core_Engine.InvokeEvent(
                            "CONFIG_Packet_Received",
                            new PacketReceivedEventArgs(packet, connection._id)
                        );
                        break;
                    case ConnectionState.PLAY:
                        Core_Engine.InvokeEvent(
                            "PLAY_Packet_Received",
                            new PacketReceivedEventArgs(packet, connection._id)
                        );
                        //DisconnectFromServer(connection._RemoteHost);
                        //Core_Engine.SignalInteractiveResetServerHolds();
                        break;
                    default:
                        Logging.LogError(
                            $"ReceiveConnections State {connection._connectionState} Not Implemented"
                        );
                        DisconnectFromServer(connection._id);
                        if (
                            _isClientConnectedToPrimaryServer
                            && _primaryClientServerConnection == connection._id
                        )
                        {
                            Core_Engine.SignalInteractiveResetServerHolds();
                        }
                        Core_Engine.SignalInteractiveResetServerHolds();
                        return;
                }
            }
            catch
            {
                DisconnectFromServer(connection._id);
                if (
                    _isClientConnectedToPrimaryServer
                    && _primaryClientServerConnection == connection._id
                )
                {
                    Core_Engine.SignalInteractiveResetServerHolds();
                }
                return;
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
}
