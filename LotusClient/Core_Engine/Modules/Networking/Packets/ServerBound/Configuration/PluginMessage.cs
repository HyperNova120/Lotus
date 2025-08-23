using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;

namespace LotusCore.Modules.Networking.Packets.ServerBound.Configuration
{
    public class PluginMessagePacket : MinecraftPacket
    {
        public Identifier? _Channel = null;
        public byte[] _Data = [];

        public PluginMessagePacket()
        {
            this._Protocol_ID = 0x02;
        }

        public override byte[] GetBytes()
        {
            return [.. StringN.GetBytes(_Channel?.IdentifierString!), .. _Data];
        }
    }
}
