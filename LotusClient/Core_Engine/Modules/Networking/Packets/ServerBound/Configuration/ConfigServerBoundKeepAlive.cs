using Core_Engine.BaseClasses.Types;

namespace Core_Engine.Modules.Networking.Packets.ServerBound.Configuration
{
    public class ConfigServerBoundKeepAlivePacket : MinecraftPacket
    {
        public long KeepAliveID;

        public ConfigServerBoundKeepAlivePacket()
        {
            protocol_id = 0x04;
        }

        public override byte[] GetBytes()
        {
            return NetworkLong.GetBytes(KeepAliveID);
        }
    }
}
