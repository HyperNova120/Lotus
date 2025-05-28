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
        private Socket? TcpSocket = null;
        public ConnectionState connectionState { get; set; } = ConnectionState.NONE;
        public Encryption encryption { get; private set; } = new();

        private SocketAsyncEventArgs socketAsyncEventArgs = new();

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

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
                        connectionState = ConnectionState.CONFIGURATION;
                    }
                )
            );
        }

        public Networking()
        {
            InitNetworking();
        }

        public void InitNetworking()
        {
            //Logging.LogDebug("InitNetworking");
            TcpSocket = null;
            connectionState = ConnectionState.NONE;
            encryption = new();
            MinecraftPacketHandler.Init();
            socketAsyncEventArgs = new();
            ResetBuffer(socketAsyncEventArgs);
            socketAsyncEventArgs.Completed += ReceiveCompleted;
        }

        public int SendPacket(MinecraftPacket packet)
        {
            if (TcpSocket == null)
            {
                return 0;
            }
            int bytesSent = 0;
            byte[] dataToSend = MinecraftPacketHandler.CreatePacket(packet);
            while (bytesSent < dataToSend.Length)
            {
                bytesSent += TcpSocket.Send(
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
            if (TcpSocket != null)
            {
                return false;
            }
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            TcpSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            TcpSocket.Connect(endPoint);
            StartReceiving(socketAsyncEventArgs);
            return true;
        }

        public void DisconnectFromServer()
        {
            if (TcpSocket == null)
            {
                return;
            }
            Logging.LogInfo("Disconnected from Server");
            TcpSocket.Disconnect(false);
            TcpSocket.Close();
            InitNetworking();
        }

        private void StartReceiving(SocketAsyncEventArgs e)
        {
            if (!TcpSocket!.ReceiveAsync(e))
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
            try
            {
                if (ProcessReceive(e))
                {
                    ResetBuffer(e);
                    if (TcpSocket != null)
                    {
                        //Logging.LogDebug("Wait for next packet");
                        StartReceiving(socketAsyncEventArgs);
                    }
                    else
                    {
                        //Logging.LogDebug("TcpSocket null");
                    }
                }
                else
                {
                    Logging.LogError($"Handle Packet Received ERROR");
                }
            }
            catch (Exception exc)
            {
                Logging.LogError($"Handle Packet Received ERROR: {exc}");
                DisconnectFromServer();
            }
        }

        private bool ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                try
                {
                    //data received properly
                    byte[] packetBytes = e.Buffer![..e.BytesTransferred];
                    /* Logging.LogDebug(
                        $"ProcessReceive {packetBytes.Length} Bytes received; State: {connectionState}"
                    ); */
                    if (packetBytes.Length == 0)
                    {
                        Logging.LogInfo("Connection Closed by Remote Host");
                        DisconnectFromServer();
                        return false;
                    }
                    if (MinecraftPacketHandler.IsEncryptionEnabled)
                    {
                        //Logging.LogDebug("Decrypting Packet");
                        packetBytes = encryption.DecryptData(packetBytes);
                    }

                    bool firstRun = true;
                    while (packetBytes.Length > 0)
                    {
                        if (!firstRun)
                        {
                            Logging.LogDebug("\tMulti packet receive");
                        }
                        (MinecraftServerPacket? serverPacket, packetBytes) =
                            MinecraftPacketHandler.DecodePacket(packetBytes);
                        if (serverPacket == null)
                        {
                            Logging.LogDebug("ReceiveConnections; bad packet, cancelling");
                            DisconnectFromServer();
                            return false;
                        }
                        ProcessPacketEvent(serverPacket);
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

        private void ProcessPacketEvent(MinecraftServerPacket packet)
        {
            /* Logging.LogDebug(
                $"\tProcessing Packet 0x{packet.protocol_id:X} in State: {connectionState.ToString()}"
            ); */
            switch (connectionState)
            {
                case ConnectionState.STATUS:
                    Core_Engine.InvokeEvent(
                        "STATUS_Packet_Received",
                        new PacketReceivedEventArgs(packet)
                    );
                    break;
                case ConnectionState.LOGIN:
                    Core_Engine.InvokeEvent(
                        "LOGIN_Packet_Received",
                        new PacketReceivedEventArgs(packet)
                    );
                    break;
                case ConnectionState.CONFIGURATION:
                    Core_Engine.InvokeEvent(
                        "CONFIG_Packet_Received",
                        new PacketReceivedEventArgs(packet)
                    );
                    break;
                default:
                    Logging.LogError($"ReceiveConnections State {connectionState} Not Implemented");
                    DisconnectFromServer();
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
