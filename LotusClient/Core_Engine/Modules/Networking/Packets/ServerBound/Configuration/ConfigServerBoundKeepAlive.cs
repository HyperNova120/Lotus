using LotusCore.BaseClasses.Types;

namespace LotusCore.Modules.Networking.Packets.ServerBound.Configuration
{
    public class ConfigServerBoundKeepAlivePacket : MinecraftPacket
    {
        public long _KeepAliveID;

        public ConfigServerBoundKeepAlivePacket()
        {
            _Protocol_ID = 0x04;
        }

        public override byte[] GetBytes()
        {
            return NetworkLong.GetBytes(_KeepAliveID);
        }
    }
}
