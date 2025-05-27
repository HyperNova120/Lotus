using System.Net;
using System.Net.Sockets;
using Core_Engine.EngineEventArgs;
using Core_Engine.Interfaces;
using Core_Engine.Modules.Networking.Internals;
using Core_Engine.Modules.Networking.Packets;
using Microsoft.Identity.Client.NativeInterop;

namespace Core_Engine.Modules.Networking
{
    public class Networking : IModuleBase
    {
        private Socket? TcpSocket = null;
        public ConnectionState connectionState { get; set; } = ConnectionState.NONE;
        public Encryption encryption { get; private set; } = new();

        private CancellationTokenSource? cancellationTokenSourse;

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

        public void RegisterEvents(Action<string> RegisterEvent)
        {
            RegisterEvent.Invoke("STATUS_Packet_Received");
            RegisterEvent.Invoke("LOGIN_Packet_Received");
        }

        public void SubscribeToEvents(Action<string, EventHandler> SubscribeToEvent) { }

        public void ExposeData(Action<string, Func<object?>> ExposeDataCallback)
        {
            ExposeDataCallback.Invoke(
                "Networking_connectionState",
                () =>
                {
                    return connectionState;
                }
            );
        }

        public Networking()
        {
            InitNetworking();
        }

        public void InitNetworking()
        {
            TcpSocket = null;
            connectionState = ConnectionState.NONE;
            encryption = new();
            MinecraftPacketHandler.Init();
            cancellationTokenSourse = new();
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
            Logging.LogDebug($"Sent {bytesSent} bytes");
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
            cancellationTokenSourse = new();
            _ = HandleIncomingPackets();
            return true;
        }

        public void DisconnectFromServer()
        {
            if (TcpSocket == null)
            {
                return;
            }
            Logging.LogDebug("Disconnect from Server");
            if (cancellationTokenSourse != null)
            {
                cancellationTokenSourse!.Cancel();
            }
            TcpSocket.EndReceive();
            TcpSocket.Disconnect(false);
            TcpSocket.Close();
            InitNetworking();
        }

        private async Task HandleIncomingPackets()
        {
            CancellationToken cancellationToken = cancellationTokenSourse!.Token;
            cancellationToken.ThrowIfCancellationRequested();

            SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
            byte[] receivedBuffer = new byte[0x3FFFFF];
            eventArgs.SetBuffer(receivedBuffer, 0, receivedBuffer.Length);
            eventArgs.Completed += (object sender, SocketAsyncEventArgs e) => { };
            try
            {
                while (TcpSocket != null && TcpSocket.Connected)
                {
                    int numBytesReceived = await TcpSocket.ReceiveAsync(
                        receivedBuffer,
                        SocketFlags.None,
                        cancellationToken
                    );
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Logging.LogDebug("Connection Cancellation requested");
                        cancellationTokenSourse = null;
                        return;
                    }
                    Logging.LogDebug("ReceiveConnections; numBytesReceived, " + numBytesReceived);
                    if (numBytesReceived == 0)
                    {
                        //connection probably closed from remote
                        Logging.LogInfo("Connection Closed by Remote Host");
                        cancellationTokenSourse = null;
                        DisconnectFromServer();
                        return;
                    }
                    byte[] packetBytes = receivedBuffer.Take(numBytesReceived).ToArray();
                    bool firstRun = true;
                    while (packetBytes.Length != 0)
                    {
                        (MinecraftServerPacket? serverPacket, packetBytes) =
                            MinecraftPacketHandler.DecodePacket(packetBytes, firstRun);

                        if (serverPacket == null)
                        {
                            Logging.LogDebug("ReceiveConnections; bad packet, cancelling");
                            cancellationTokenSourse = null;
                            DisconnectFromServer();
                            return;
                        }
                        switch (connectionState)
                        {
                            case ConnectionState.STATUS:
                                Core_Engine.InvokeEvent(
                                    "STATUS_Packet_Received",
                                    new PacketReceivedEventArgs(serverPacket)
                                );
                                break;
                            case ConnectionState.LOGIN:
                                Core_Engine.InvokeEvent(
                                    "LOGIN_Packet_Received",
                                    new PacketReceivedEventArgs(serverPacket)
                                );
                                break;
                            default:
                                Logging.LogError(
                                    $"ReceiveConnections State {connectionState} Not Implemented"
                                );
                                DisconnectFromServer();
                                return;
                        }
                        firstRun = false;
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogDebug("Connection Failed");
                Logging.LogError(e.ToString());
                DisconnectFromServer();
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
