
using LotusCore.Modules.Networking.Packets;

namespace LotusCore.Modules.Networking.Packets.ServerBound.Status
{
    public class StatusRequestPacket : MinecraftPacket
    {
        public override byte[] GetBytes()
        {
            return [];
        }
    }
}
