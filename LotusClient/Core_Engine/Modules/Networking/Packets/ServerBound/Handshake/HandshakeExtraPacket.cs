namespace LotusCore.Modules.Networking.Packets.ServerBound.Handshake
{
    public class HandshakeExtraPacket : MinecraftPacket
    {
        public override byte[] GetBytes()
        {
            return [(byte)0x01];
        }

        public HandshakeExtraPacket()
        {
            _Protocol_ID = 0xFE;
        }
    }
}
