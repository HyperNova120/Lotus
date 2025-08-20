using System.Text;
using Core_Engine.BaseClasses.Types;
using Core_Engine.Modules.GameStateHandler.BaseClasses;
using static Core_Engine.Modules.Networking.Packets.ClientBound.Configuration.ConfigClientboundKnownPacks;

namespace Core_Engine.Modules.Networking.Packets.ServerBound.Configuration
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
