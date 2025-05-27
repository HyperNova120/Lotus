namespace Core_Engine.Modules.Networking.Packets
{
    public class EmptyPacket : MinecraftPacket
    {
        public EmptyPacket(int protocol)
        {
            this.protocol_id = protocol;
        }

        public override byte[] GetBytes()
        {
            return [];
        }
    }
}
