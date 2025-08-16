using Core_Engine.BaseClasses.Types;

namespace Core_Engine.Modules.Networking.Packets.ServerBound.Configuration
{
    public class ConfigResourcePackResponse : MinecraftPacket
    {
        public UInt128 UUID;
        public ConfigResourcePackResponseResult Result;

        public ConfigResourcePackResponse()
        {
            protocol_id = 0x06;
        }

        public override byte[] GetBytes()
        {
            return [.. NetworkUUID.GetNetworkBytes(UUID), .. VarInt_VarLong.EncodeInt((int)Result)];
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
