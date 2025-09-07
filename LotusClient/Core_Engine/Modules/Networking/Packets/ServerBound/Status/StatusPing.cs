using LotusCore.BaseClasses.Types;
using LotusCore.Modules.Networking.Packets;

namespace LotusCore.Modules.Networking.Packets.ServerBound.Status
{
    public class StatusPingRequestPacket : MinecraftPacket
    {
        public override byte[] GetBytes()
        {
            return [.. NetworkLong.GetBytes(DateTime.UtcNow.Millisecond)];
        }

        public StatusPingRequestPacket()
        {
            _Protocol_ID = 0x01;
        }
    }
}
