using LotusCore.BaseClasses.Types;

namespace LotusCore.Modules.Networking.Packets.ClientBound.Configuration;

public class ConfigClientboundKnownPacks
{
    public List<PackInfo> _KnownPacks = new();

    public int DecodeFromBytes(byte[] bytes)
    {
        (int arraySize, int offset) = PrefixedArray.GetSizeOfArray(bytes);
        for (int i = 0; i < arraySize; i++)
        {
            PackInfo tmp = new();
            (tmp.Namespace, int numBytes) = StringN.DecodeBytes(bytes[offset..]);
            offset += numBytes;
            (tmp.ID, int numBytes2) = StringN.DecodeBytes(bytes[offset..]);
            offset += numBytes2;
            (tmp.Version, int numBytes3) = StringN.DecodeBytes(bytes[offset..]);
            offset += numBytes3;
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
