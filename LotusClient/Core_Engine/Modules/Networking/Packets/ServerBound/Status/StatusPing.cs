using LotusCore.BaseClasses.Types;
using LotusCore.Modules.LotusNetty.Packets;

namespace LotusCore.Modules.LotusNetty.Packets.ServerBound.Status
{
    public class StatusPingRequestPacket : MinecraftPacket
    {
        public override byte[] GetBytes()
        {
            return [.. NetworkLong.GetBytes(DateTime.UtcNow.Millisecond)];
        }

        public StatusPingRequestPacket()
        {
            _protocol_ID = 0x01;
        }
    }
}
