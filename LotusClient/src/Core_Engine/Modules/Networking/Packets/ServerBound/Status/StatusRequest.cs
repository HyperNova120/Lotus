
using LotusCore.Modules.LotusNetty.Packets;

namespace LotusCore.Modules.LotusNetty.Packets.ServerBound.Status
{
    public class StatusRequestPacket : MinecraftPacket
    {
        public override byte[] GetBytes()
        {
            return [];
        }
    }
}
