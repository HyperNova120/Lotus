namespace MinecraftNetworking.Packets
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
