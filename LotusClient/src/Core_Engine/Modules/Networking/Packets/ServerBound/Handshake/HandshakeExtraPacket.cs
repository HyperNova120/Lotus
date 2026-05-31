namespace LotusCore.Modules.LotusNetty.Packets.ServerBound.Handshake
{
    public class HandshakeExtraPacket : MinecraftPacket
    {
        public override byte[] GetBytes()
        {
            return [(byte)0x01];
        }

        public HandshakeExtraPacket()
        {
            _protocol_ID = 0xFE;
        }
    }
}
