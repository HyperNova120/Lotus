using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.Networking.Internals;
using LotusCore.Modules.Networking.Packets;
using LotusCore.Modules.Networking.Packets.ServerBound.Configuration;
using Microsoft.Identity.Client.NativeInterop;
using Org.BouncyCastle.Asn1.Icao;

namespace LotusCore.Modules.Networking
{
    public class Networking : IModuleBase
    {
        private Dictionary<IPAddress, ServerConnection> _Connections = new();
        public bool _IsClientConnectedToPrimaryServer { get; set; } = false;
        private IPAddress? _PrimaryClientServerConnection;
        public readonly ProtocolVersionUtils.ProtocolVersion _ProtocolVersion = ProtocolVersionUtils
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
        }

        public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent)
        {
            SubscribeToEvent.Invoke(
                "SERVERLOGIN_loginSuccessful",
                new EngineEventHandler(
                    (sender, args) =>
                    {
                        ConnectionEventArgs conArgs = (ConnectionEventArgs)args;
                        GetServerConnection(conArgs._RemoteHost)!._ConnectionState =
                            ConnectionState.CONFIGURATION;
                        _PrimaryClientServerConnection = conArgs._RemoteHost;
                        _IsClientConnectedToPrimaryServer = true;
                        return null;
                    }
                )
            );
        }

        public int SendPacket(
            IPAddress RemoteHost,
            MinecraftPacket packet,
            bool HoldPacketInBuffer = false
        )
        {
            ServerConnection? connection = GetServerConnection(RemoteHost);
            if (connection == null)
            {
                Logging.LogError("Attempting to send packet to null connection");
                return -1;
            }

            if (connection._TcpSocket == null)
            {
                return -1;
            }
            connection._DataToSendBuffer.AddRange(
                connection._MinecraftPacketHandler.CreatePacket(connection, packet)
            );

            if (HoldPacketInBuffer)
            {
                return 0;
            }

            return SendBufferedPackets(connection);
        }

        public int SendPacket(
            IPAddress RemoteHost,
            IEnumerable<MinecraftPacket> packets,
            bool HoldPacketInBuffer = false
        )
        {
            ServerConnection? connection = GetServerConnection(RemoteHost);
            if (connection == null)
            {
                Logging.LogError("Attempting to send packet to null connection");
                return -1;
            }

            if (connection._TcpSocket == null)
            {
                return -1;
            }

            foreach (MinecraftPacket minecraftPacket in packets)
            {
                connection._DataToSendBuffer.AddRange(
                    connection._MinecraftPacketHandler.CreatePacket(connection, minecraftPacket)
                );
            }

            if (HoldPacketInBuffer)
            {
                return 0;
            }

            return SendBufferedPackets(connection);
        }

        public int SendBufferedPackets(IPAddress RemoteHost)
        {
            return SendBufferedPackets(GetServerConnection(RemoteHost));
        }

        public int SendBufferedPackets(ServerConnection? connection)
        {
            if (connection == null)
            {
                Logging.LogError("Attempting to send packet to null connection");
                return -1;
            }

            if (connection._TcpSocket == null)
            {
                return -1;
            }

            int bytesSent = 0;

            while (bytesSent < connection._DataToSendBuffer.Count)
            {
                bytesSent += connection._TcpSocket.Send(
                    connection._DataToSendBuffer.ToArray(),
                    bytesSent,
                    connection._DataToSendBuffer.Count - bytesSent,
                    SocketFlags.None
                );
            }
            //Logging.LogDebug($"Sent {bytesSent} bytes");
            connection._DataToSendBuffer.Clear();
            return bytesSent;
        }

        public bool ConnectToServer(string ip, int port = 25565)
        {
            ServerConnection serverConnection = new(ip);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            serverConnection._TcpSocket = new Socket(
                endPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
            );
            serverConnection._TcpSocket.NoDelay = true;
            serverConnection._TcpSocket.Connect(endPoint);
            _Connections[serverConnection._RemoteHost] = serverConnection;

            ResetBuffer(serverConnection._ServerConnectionSocketAsyncEventArgs);
            serverConnection._ServerConnectionSocketAsyncEventArgs.Completed += ReceiveCompleted;

            StartReceiving(serverConnection._ServerConnectionSocketAsyncEventArgs);
            return true;
        }

        public void DisconnectFromServer(IPAddress remoteHost)
        {
            ServerConnection? connection = GetServerConnection(remoteHost);
            if (connection == null)
            {
                return;
            }
            Logging.LogInfo("Disconnected from Server:" + remoteHost);
            if (
                _IsClientConnectedToPrimaryServer
                && connection._RemoteHost == _PrimaryClientServerConnection
            )
            {
                _IsClientConnectedToPrimaryServer = false;
                _PrimaryClientServerConnection = null;
            }
            if (connection!._TcpSocket != null)
            {
                connection!._TcpSocket!.Disconnect(false);
                connection!._TcpSocket!.Close();
                connection!._TcpSocket = null;
            }
            _Connections.Remove(remoteHost);

            /* if (Core_Engine.CurrentState == Core_Engine.State.Waiting)
            {
                Core_Engine.CurrentState = Core_Engine.State.Interactive;
            } */
            return;
        }

        public ServerConnection? GetServerConnection(IPAddress remoteHost)
        {
            if (_Connections.ContainsKey(remoteHost))
            {
                return _Connections[remoteHost];
            }
            return null;
        }

        private void StartReceiving(SocketAsyncEventArgs e)
        {
            ServerConnectionSocketAsyncEventArgs eventArgs =
                (ServerConnectionSocketAsyncEventArgs)e;
            ServerConnection? connection = GetServerConnection(eventArgs._RemoteHost);
            if (connection == null)
            {
                Logging.LogError("Server Connection Null");
                return;
            }
            if (!connection._TcpSocket!.ReceiveAsync(e))
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
            Logging.LogDebug("ReceiveCompleted");
            ServerConnectionSocketAsyncEventArgs eventArgs =
                (ServerConnectionSocketAsyncEventArgs)e;
            ServerConnection connection = GetServerConnection(eventArgs._RemoteHost)!;
            try
            {
                if (ProcessReceive(e))
                {
                    ResetBuffer(e);
                    if (connection._TcpSocket != null)
                    {
                        //Logging.LogDebug("Wait for next packet");
                        StartReceiving(e);
                    }
                    else
                    {
                        //Logging.LogDebug("TcpSocket null");
                    }
                }
                else
                {
                    //Logging.LogError($"Handle Packet Received ERROR");
                    Core_Engine.SignalInteractiveResetServerHolds();
                }
            }
            catch (Exception exc)
            {
                Logging.LogError($"Handle Packet Received ERROR: {exc}");
                DisconnectFromServer(connection._RemoteHost);
                Core_Engine.SignalInteractiveResetServerHolds();
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
                    ServerConnection connection = GetServerConnection(eventArgs._RemoteHost)!;
                    //data received properly
                    byte[] tmpBuffer = e.Buffer![..e.BytesTransferred];
                    /* Logging.LogDebug(
                        $"ProcessReceive {packetBytes.Length} Bytes received; State: {connectionState}"
                    ); */
                    if (tmpBuffer.Length == 0)
                    {
                        Logging.LogInfo("Connection Closed by Remote Host");
                        DisconnectFromServer(eventArgs._RemoteHost);
                        return false;
                    }
                    if (connection._MinecraftPacketHandler._IsEncryptionEnabled)
                    {
                        //Logging.LogDebug("Decrypting Packet");
                        tmpBuffer = connection._Encryption.DecryptData(tmpBuffer);
                    }

                    connection._IncompletePacketBytesBuffer =
                    [
                        .. connection._IncompletePacketBytesBuffer,
                        .. tmpBuffer,
                    ];

                    bool firstRun = true;
                    while (connection._IncompletePacketBytesBuffer.Length > 0)
                    {
                        if (!firstRun)
                        {
                            Logging.LogDebug("\tMulti packet receive");
                        }
                        (
                            MinecraftServerPacket? serverPacket,
                            connection._IncompletePacketBytesBuffer
                        ) = connection._MinecraftPacketHandler.DecodePacket(
                            connection._RemoteHost,
                            connection._IncompletePacketBytesBuffer
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
            Logging.LogDebug(
                $"\tProcessing Packet 0x{packet._Protocol_ID:X} in State: {connection._ConnectionState.ToString()}"
            );
            switch (connection._ConnectionState)
            {
                case ConnectionState.STATUS:
                    Core_Engine.InvokeEvent(
                        "STATUS_Packet_Received",
                        new PacketReceivedEventArgs(packet, connection._RemoteHost)
                    );
                    break;
                case ConnectionState.LOGIN:
                    Core_Engine.InvokeEvent(
                        "LOGIN_Packet_Received",
                        new PacketReceivedEventArgs(packet, connection._RemoteHost)
                    );
                    break;
                case ConnectionState.CONFIGURATION:
                    Core_Engine.InvokeEvent(
                        "CONFIG_Packet_Received",
                        new PacketReceivedEventArgs(packet, connection._RemoteHost)
                    );
                    break;
                case ConnectionState.PLAY:
                    Core_Engine.InvokeEvent(
                        "PLAY_Packet_Received",
                        new PacketReceivedEventArgs(packet, connection._RemoteHost)
                    );
                    break;
                default:
                    Logging.LogError(
                        $"ReceiveConnections State {connection._ConnectionState} Not Implemented"
                    );
                    DisconnectFromServer(connection._RemoteHost);
                    Core_Engine.SignalInteractiveResetServerHolds();
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
