namespace Core_Engine.Modules.Networking.Packets
{
    public class EmptyPacket : MinecraftPacket
    {
        public EmptyPacket(int protocol)
        {
            this._Protocol_ID = protocol;
        }

        public override byte[] GetBytes()
        {
            return [];
        }
    }
}
