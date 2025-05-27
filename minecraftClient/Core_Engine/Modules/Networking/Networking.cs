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
            _ = HandleIncomingPackets();
            return true;
        }

        private async Task HandleIncomingPackets()
        {
            try
            {
                byte[] receivedBuffer = new byte[0x3FFFFF];
                while (TcpSocket != null && TcpSocket.Connected)
                {
                    int numBytesReceived = await TcpSocket.ReceiveAsync(
                        receivedBuffer,
                        SocketFlags.None
                    );
                    Logging.LogDebug("ReceiveConnections; numBytesReceived, " + numBytesReceived);
                    if (numBytesReceived == 0)
                    {
                        //connection probably closed from remote
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
                                break;
                        }
                        firstRun = false;
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogError(e.ToString());
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
