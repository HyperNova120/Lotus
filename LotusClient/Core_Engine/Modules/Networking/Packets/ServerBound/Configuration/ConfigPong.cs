using Core_Engine.BaseClasses.Types;

namespace Core_Engine.Modules.Networking.Packets.ServerBound.Configuration
{
    public class ConfigPongPacket : MinecraftPacket
    {
        public int ID;

        public ConfigPongPacket()
        {
            protocol_id = 0x05;
        }

        public override byte[] GetBytes()
        {
            return NetworkInt.GetBytes(ID);
        }
    }
}
