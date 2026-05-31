using System.Text;
using LotusCore.BaseClasses.Types;
using LotusCore.Modules.GameStateHandlerModule.BaseClasses;
using static LotusCore.Modules.LotusNetty.Packets.ClientBound.Configuration.ConfigClientboundKnownPacks;

namespace LotusCore.Modules.LotusNetty.Packets.ServerBound.Configuration
{
    public class ServerboundKnownPacksPacket : MinecraftPacket
    {
        public List<PackInfo> _KnownPacks = new();

        public ServerboundKnownPacksPacket()
        {
            _protocol_ID = 0x07;
        }

        public override byte[] GetBytes()
        {
            List<byte> tmp = new();
            foreach (var p in _KnownPacks)
            {
                tmp.AddRange(StringN.GetBytes(p.Namespace));
                tmp.AddRange(StringN.GetBytes(p.ID));
                tmp.AddRange(StringN.GetBytes(p.Version));
            }
            return PrefixedArray.GetBytes(_KnownPacks.Count, [.. tmp]);
        }
    }
}
