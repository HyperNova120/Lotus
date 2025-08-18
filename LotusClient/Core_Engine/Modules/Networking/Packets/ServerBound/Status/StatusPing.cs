
using Core_Engine.Modules.Networking.Packets;

namespace Core_Engine.Modules.Networking.Packets.ServerBound.Status
{
    public class StatusPingRequestPacket : MinecraftPacket
    {
        public override byte[] GetBytes()
        {
            return [.. BitConverter.GetBytes(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond)];
        }

        public StatusPingRequestPacket()
        {
            _Protocol_ID = 0x01;
        }
    }
}
