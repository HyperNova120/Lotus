using System.Text;
using MinecraftNetworking.Connection;
using MinecraftNetworking.Packets;
using MinecraftNetworking.Types;

namespace MinecraftNetworking.StateHandlers
{
    public static class StatusHandler
    {
        public static async Task ProcessPacket(MinecraftServerPacket packet)
        {
            switch (packet.protocol_id)
            {
                case 0x00:
                    _ = HandleStatusResponse(packet);
                    break;
                case 0x01:
                    _ = HandleStatusPingResponse(packet);
                    break;
                default:
                    Logging.LogError(
                        $"StatusHandler State 0x{packet.protocol_id:X} Not Implemented"
                    );
                    break;
            }
        }

        private static async Task HandleStatusPingResponse(MinecraftServerPacket packet)
        {
            try
            {
                long value = BitConverter.ToInt64(packet.data);
                Logging.LogDebug(
                    $"Response: {value} Ping:{(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - value}m"
                );
            }
            catch (Exception e)
            {
                Logging.LogDebug(e.ToString());
            }
        }

        private static async Task HandleStatusResponse(MinecraftServerPacket packet)
        {
            (string value, int size) = StringN.DecodeBytes(packet.data);
            Logging.LogDebug($"Response Size: {size}\n{value}");
        }

        public static void SendStatusRequest(string serverIp, ushort ip = 25565)
        {
            if (ConnectionHandler.connectionState == ConnectionState.NONE)
            {
                ConnectionHandler.connectionState = ConnectionState.STATUS;
                ConnectionHandler.SendPacket(
                    new HandshakePacket(serverIp, HandshakePacket.Intent.Status, ip)
                );
                ConnectionHandler.SendPacket(new StatusRequestPacket());
            }
        }

        public static void SendPingRequest()
        {
            if (ConnectionHandler.connectionState == ConnectionState.STATUS)
            {
                ConnectionHandler.SendPacket(new StatusPingRequestPacket());
            }
        }
    }
}
