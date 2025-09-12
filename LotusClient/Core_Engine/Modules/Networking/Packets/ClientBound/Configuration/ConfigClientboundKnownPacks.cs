using LotusCore.BaseClasses.Types;

namespace LotusCore.Modules.Networking.Packets.ClientBound.Configuration;

public class ConfigClientboundKnownPacks
{
    public List<PackInfo> _KnownPacks = new();

    public int DecodeFromBytes(byte[] bytes)
    {
        int offset = 0;
        int arraySize = PrefixedArray.GetSizeOfArray(bytes, ref offset);
        for (int i = 0; i < arraySize; i++)
        {
            PackInfo tmp = new();
            tmp.Namespace = StringN.DecodeBytes(bytes, ref offset);
            tmp.ID = StringN.DecodeBytes(bytes, ref offset);
            tmp.Version = StringN.DecodeBytes(bytes, ref offset);
            _KnownPacks.Add(tmp);
        }
        return offset;
    }

    public struct PackInfo
    {
        public string Namespace;
        public string ID;
        public string Version;
    }
}
