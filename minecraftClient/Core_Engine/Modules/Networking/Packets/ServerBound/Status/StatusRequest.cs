
using Core_Engine.Modules.Networking.Packets;

namespace Core_Engine.Modules.Networking.Pakcets.ServerBound.Status
{
    public class StatusRequestPacket : MinecraftPacket
    {
        public override byte[] GetBytes()
        {
            return [];
        }
    }
}
