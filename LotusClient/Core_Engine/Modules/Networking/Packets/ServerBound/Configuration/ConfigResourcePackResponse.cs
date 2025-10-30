using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;

namespace LotusCore.Modules.LotusNetty.Packets.ServerBound.Configuration
{
    public class ConfigResourcePackResponse : MinecraftPacket
    {
        public MinecraftUUID _UUID;
        public ConfigResourcePackResponseResult _result;

        public ConfigResourcePackResponse()
        {
            _protocol_ID = 0x06;
        }

        public override byte[] GetBytes()
        {
            return [.. _UUID.GetBytes(), .. VarInt_VarLong.EncodeInt((int)_result)];
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
