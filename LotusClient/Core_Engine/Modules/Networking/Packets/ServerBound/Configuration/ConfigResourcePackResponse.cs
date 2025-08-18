using Core_Engine.BaseClasses.Types;

namespace Core_Engine.Modules.Networking.Packets.ServerBound.Configuration
{
    public class ConfigResourcePackResponse : MinecraftPacket
    {
        public UInt128 _UUID;
        public ConfigResourcePackResponseResult _Result;

        public ConfigResourcePackResponse()
        {
            _Protocol_ID = 0x06;
        }

        public override byte[] GetBytes()
        {
            return [.. NetworkUUID.GetNetworkBytes(_UUID), .. VarInt_VarLong.EncodeInt((int)_Result)];
        }

        public enum ConfigResourcePackResponseResult
        {
            SUCCESSFULLY_DOWNLOADED,
            DECLINED,
            FAILED_TO_DOWNLOAD,
            ACCEPTED,
            DOWNLOADED,
            INVALID_URL,
            FAILED_TO_RELOAD,
            DISCARDED,
        }
    }
}
