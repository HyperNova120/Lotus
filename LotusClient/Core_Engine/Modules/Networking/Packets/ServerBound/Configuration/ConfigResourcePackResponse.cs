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
            Successfully_Downloaded,
            Declined,
            Failed_To_Download,
            Accepted,
            Downloaded,
            Invalid_URL,
            Failed_To_Reload,
            Discarded,
        }
    }
}
