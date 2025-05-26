using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Identity.Client.NativeInterop;
using MinecraftNetworking.StateHandlers;

namespace MinecraftNetworking.Connection
{
    public static class ConnectionHandler
    {
        private static Socket? TcpSocket;
        public static ConnectionState connectionState = ConnectionState.NONE;

        public static bool ConnectToServer(string ip, int port = 25565)
        {
            if (TcpSocket != null)
            {
                return false;
            }
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            TcpSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            TcpSocket.Connect(endPoint);
            _ = ReceiveConnections(port);
            return true;
        }

        public static void DisconnectSocket()
        {
            TcpSocket!.Disconnect(false);
            TcpSocket = null;
        }

        public static int SendPacket(MinecraftPacket packet)
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

        public static async Task ReceiveConnections(int port)
        {
            try
            {
                Logging.LogDebug($"Accepted Connection");

                byte[] receivedBuffer = new byte[0x3FFFFF];
                while (TcpSocket != null && TcpSocket.Connected)
                {
                    int numBytesReceived = await TcpSocket.ReceiveAsync(
                        receivedBuffer,
                        SocketFlags.None
                    );
                    if (numBytesReceived != 0)
                    {
                        Logging.LogDebug($"Received {numBytesReceived} Bytes");
                        byte[] packetBytes = receivedBuffer.Take(numBytesReceived).ToArray();
                        MinecraftServerPacket serverPacket = MinecraftPacketHandler.DecodePacket(
                            packetBytes
                        );
                        switch (connectionState)
                        {
                            case ConnectionState.STATUS:
                                _ = StatusHandler.ProcessPacket(serverPacket);
                                break;
                            case ConnectionState.LOGIN:
                                _ = LoginHandler.ProcessPacket(serverPacket);
                                break;
                            default:
                                Logging.LogError(
                                    $"ReceiveConnections State {connectionState} Not Implemented"
                                );
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogError("ReceiveConnections ERROR:" + e.ToString());
            }
        }
    }
}
