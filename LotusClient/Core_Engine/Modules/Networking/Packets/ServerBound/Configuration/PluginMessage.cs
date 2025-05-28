using Core_Engine.Modules.Networking.Types;

namespace Core_Engine.Modules.Networking.Packets.ServerBound.Configuration
{
    public class PluginMessagePacket : MinecraftPacket
    {
        public string Channel = "";
        public byte[] Data = [];

        public PluginMessagePacket()
        {
            this.protocol_id = 0x02;
        }

        public override byte[] GetBytes()
        {
            return [.. StringN.GetBytes(Channel), .. Data];
        }
    }
}
