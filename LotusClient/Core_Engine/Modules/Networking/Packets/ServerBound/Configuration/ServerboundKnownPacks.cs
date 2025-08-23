using System.Text;
using LotusCore.BaseClasses.Types;
using LotusCore.Modules.GameStateHandler.BaseClasses;
using static LotusCore.Modules.Networking.Packets.ClientBound.Configuration.ConfigClientboundKnownPacks;

namespace LotusCore.Modules.Networking.Packets.ServerBound.Configuration
{
    public class ServerboundKnownPacksPacket : MinecraftPacket
    {
        public List<PackInfo> _KnownPacks = new();

        public ServerboundKnownPacksPacket()
        {
            _Protocol_ID = 0x07;
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
