using System.Text;
using Core_Engine.Modules.Networking.Types;

namespace Core_Engine.Modules.Networking.Packets.ServerBound.Configuration
{
    public class ServerboundKnownPacksPacket : MinecraftPacket
    {
        public List<DataPack> KnownPacks = new();

        public ServerboundKnownPacksPacket()
        {
            protocol_id = 0x07;
        }

        public override byte[] GetBytes()
        {
            List<byte> bytes = new();
            foreach (DataPack b in KnownPacks)
            {
                bytes.AddRange(StringN.GetBytes(b.S1));
                bytes.AddRange(StringN.GetBytes(b.S2));
                bytes.AddRange(StringN.GetBytes(b.S3));
            }
            return PrefixedArray.GetBytes(KnownPacks.Count, bytes.ToArray());
        }

        public class DataPack
        {
            public string S1 = "";
            public string S2 = "";
            public string S3 = "";
        }
    }
}
