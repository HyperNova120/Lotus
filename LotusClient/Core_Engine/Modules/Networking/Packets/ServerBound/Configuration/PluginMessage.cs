using Core_Engine.BaseClasses;
using Core_Engine.BaseClasses.Types;

namespace Core_Engine.Modules.Networking.Packets.ServerBound.Configuration
{
    public class PluginMessagePacket : MinecraftPacket
    {
        public Identifier? Channel = null;
        public byte[] Data = [];

        public PluginMessagePacket()
        {
            this.protocol_id = 0x02;
        }

        public override byte[] GetBytes()
        {
            return [.. StringN.GetBytes(Channel.IdentifierString ?? ""), .. Data];
        }
    }
}
