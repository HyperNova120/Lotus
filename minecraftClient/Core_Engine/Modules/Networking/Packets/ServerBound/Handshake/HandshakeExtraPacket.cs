
using Core_Engine.Modules.Networking.Packets;

namespace Core_Engine.Modules.Networking.Pakcets.ServerBound.Handshake
{
    public class HandshakeExtraPacket : MinecraftPacket
    {
        public override byte[] GetBytes()
        {
            return [(byte)0x01];
        }
        public HandshakeExtraPacket()
        {
            protocol_id = 0xFE;
        }
    }
}
