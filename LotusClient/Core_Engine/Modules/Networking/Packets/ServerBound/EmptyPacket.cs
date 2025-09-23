namespace LotusCore.Modules.Networking.Packets
{
    public class EmptyPacket : MinecraftPacket
    {
        public EmptyPacket(int protocol_ID)
        {
            this._protocol_ID = protocol_ID;
        }

        public override byte[] GetBytes()
        {
            return [];
        }
    }
}
