using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Core_Engine.EngineEventArgs;
using Core_Engine.Interfaces;
using Core_Engine.Modules.Networking.Internals;
using Core_Engine.Modules.Networking.Packets;
using Core_Engine.Modules.Networking.Packets.ServerBound.Configuration;
using Microsoft.Identity.Client.NativeInterop;
using Org.BouncyCastle.Asn1.Icao;

namespace Core_Engine.Modules.Networking
{
    public class Networking : IModuleBase
    {
        private Dictionary<IPAddress, ServerConnection> Connections = new();
        public bool IsClientConnectedToPrimaryServer { get; set; } = false;
        private IPAddress? PrimaryClientServerConnection;
        public static readonly ProtocolVersion protocolVersion = ProtocolVersion.V1_21_8;

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

        private byte[] packetBytes = [];

        public void RegisterEvents(Action<string> RegisterEvent)
        {
            RegisterEvent.Invoke("STATUS_Packet_Received");
            RegisterEvent.Invoke("LOGIN_Packet_Received");
            RegisterEvent.Invoke("CONFIG_Packet_Received");
        }

        public void SubscribeToEvents(Action<string, EventHandler> SubscribeToEvent)
        {
            SubscribeToEvent.Invoke(
                "SERVERLOGIN_loginSuccessful",
                new EventHandler(
                    (sender, args) =>
                    {
                        ConnectionEventArgs conArgs = (ConnectionEventArgs)args;
                        GetServerConnection(conArgs.remoteHost)!.connectionState =
                            ConnectionState.CONFIGURATION;
                        PrimaryClientServerConnection = conArgs.remoteHost;
                        IsClientConnectedToPrimaryServer = true;
                    }
                )
            );
        }

        public int SendPacket(IPAddress RemoteHost, MinecraftPacket packet)
        {
            ServerConnection? connection = GetServerConnection(RemoteHost);
            if (connection == null)
            {
                Logging.LogError("Attempting to send packet to null connection");
                return -1;
            }

            if (connection.TcpSocket == null)
            {
                return -1;
            }
            int bytesSent = 0;
            byte[] dataToSend = connection.minecraftPacketHandler.CreatePacket(connection, packet);
            while (bytesSent < dataToSend.Length)
            {
                bytesSent += connection.TcpSocket.Send(
                    dataToSend,
                    bytesSent,
                    dataToSend.Length - bytesSent,
                    SocketFlags.None
                );
            }
            //Logging.LogDebug($"Sent {bytesSent} bytes");
            return bytesSent;
        }

        public bool ConnectToServer(string ip, int port = 25565)
        {
            ServerConnection serverConnection = new(ip);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            serverConnection.TcpSocket = new Socket(
                endPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
            );
            serverConnection.TcpSocket.Connect(endPoint);
            Connections[serverConnection.remoteHost] = serverConnection;

            ResetBuffer(serverConnection.serverConnectionSocketAsyncEventArgs);
            serverConnection.serverConnectionSocketAsyncEventArgs.Completed += ReceiveCompleted;

            StartReceiving(serverConnection.serverConnectionSocketAsyncEventArgs);
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
                IsClientConnectedToPrimaryServer
                && connection.remoteHost == PrimaryClientServerConnection
            )
            {
                IsClientConnectedToPrimaryServer = false;
                PrimaryClientServerConnection = null;
            }
            if (connection!.TcpSocket != null)
            {
                connection!.TcpSocket!.Disconnect(false);
                connection!.TcpSocket!.Close();
                connection!.TcpSocket = null;
            }
            Connections.Remove(remoteHost);

            /* if (Core_Engine.CurrentState == Core_Engine.State.Waiting)
            {
                Core_Engine.CurrentState = Core_Engine.State.Interactive;
            } */
            return;
        }

        public ServerConnection? GetServerConnection(IPAddress remoteHost)
        {
            if (Connections.ContainsKey(remoteHost))
            {
                return Connections[remoteHost];
            }
            return null;
        }

        private void StartReceiving(SocketAsyncEventArgs e)
        {
            ServerConnectionSocketAsyncEventArgs eventArgs =
                (ServerConnectionSocketAsyncEventArgs)e;
            ServerConnection? connection = GetServerConnection(eventArgs.remoteHost);
            if (connection == null)
            {
                Logging.LogError("Server Connection Null");
                return;
            }
            if (!connection.TcpSocket!.ReceiveAsync(e))
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
            ServerConnection connection = GetServerConnection(eventArgs.remoteHost)!;
            try
            {
                if (ProcessReceive(e))
                {
                    ResetBuffer(e);
                    if (connection.TcpSocket != null)
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
                    Logging.LogError($"Handle Packet Received ERROR");
                    Core_Engine.SignalInteractiveResetServerHolds();
                }
            }
            catch (Exception exc)
            {
                Logging.LogError($"Handle Packet Received ERROR: {exc}");
                DisconnectFromServer(connection.remoteHost);
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
                    ServerConnection connection = GetServerConnection(eventArgs.remoteHost)!;
                    //data received properly
                    byte[] tmpBuffer = e.Buffer![..e.BytesTransferred];
                    /* Logging.LogDebug(
                        $"ProcessReceive {packetBytes.Length} Bytes received; State: {connectionState}"
                    ); */
                    if (tmpBuffer.Length == 0)
                    {
                        Logging.LogInfo("Connection Closed by Remote Host");
                        DisconnectFromServer(eventArgs.remoteHost);
                        return false;
                    }
                    if (connection.minecraftPacketHandler.IsEncryptionEnabled)
                    {
                        //Logging.LogDebug("Decrypting Packet");
                        tmpBuffer = connection.encryption.DecryptData(tmpBuffer);
                    }

                    packetBytes = [.. packetBytes, .. tmpBuffer];

                    bool firstRun = true;
                    while (packetBytes.Length > 0)
                    {
                        if (!firstRun)
                        {
                            Logging.LogDebug("\tMulti packet receive");
                        }
                        (MinecraftServerPacket? serverPacket, packetBytes) =
                            connection.minecraftPacketHandler.DecodePacket(
                                connection.remoteHost,
                                packetBytes
                            );
                        if (serverPacket == null)
                        {
                            Logging.LogDebug(
                                $"ReceiveConnections; bad packet, Remaining Size:{packetBytes.Length}"
                            );
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
                $"\tProcessing Packet 0x{packet.protocol_id:X} in State: {connection.connectionState.ToString()}"
            );
            switch (connection.connectionState)
            {
                case ConnectionState.STATUS:
                    Core_Engine.InvokeEvent(
                        "STATUS_Packet_Received",
                        new PacketReceivedEventArgs(packet, connection.remoteHost)
                    );
                    break;
                case ConnectionState.LOGIN:
                    Core_Engine.InvokeEvent(
                        "LOGIN_Packet_Received",
                        new PacketReceivedEventArgs(packet, connection.remoteHost)
                    );
                    break;
                case ConnectionState.CONFIGURATION:
                    Core_Engine.InvokeEvent(
                        "CONFIG_Packet_Received",
                        new PacketReceivedEventArgs(packet, connection.remoteHost)
                    );
                    break;
                default:
                    Logging.LogError(
                        $"ReceiveConnections State {connection.connectionState} Not Implemented"
                    );
                    DisconnectFromServer(connection.remoteHost);
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
