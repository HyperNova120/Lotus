using LotusCore.BaseClasses.Types;

namespace LotusCore.Modules.Networking.Packets.ServerBound.Configuration
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
