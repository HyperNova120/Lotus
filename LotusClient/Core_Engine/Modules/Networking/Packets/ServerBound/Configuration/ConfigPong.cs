using Core_Engine.BaseClasses.Types;

namespace Core_Engine.Modules.Networking.Packets.ServerBound.Configuration
{
    public class ConfigPongPacket : MinecraftPacket
    {
        public int _ID;

        public ConfigPongPacket()
        {
            _Protocol_ID = 0x05;
        }

        public override byte[] GetBytes()
        {
            return NetworkInt.GetBytes(_ID);
        }
    }
}
