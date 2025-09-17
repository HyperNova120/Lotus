using LotusCore.BaseClasses.Types;
using LotusCore.Modules.Networking.Packets.ClientBound.Login.Internals;

namespace LotusCore.Modules.Networking.Packets.ClientBound.Login
{
    public class LoginSuccessPacket
    {
        public Guid _uuid;
        public string? _Username;
        public List<LoginSuccessPacketElement> _Elements = new();

        /// <summary>
        /// populates packet based on data
        /// </summary>
        /// <param name="data"></param>
        /// <returns>number of bytes read</returns>
        public int DecodeFromBytes(byte[] data)
        {
            (byte[] uuidBytes, _) = NetworkUUID.DecodeNetworkBytes(data[0..16]);
            _uuid = new Guid(uuidBytes);
            data = data[16..];
            int offset = 0;
            _Username = StringN.DecodeBytes(data, ref offset);
            int arraySize = PrefixedArray.GetSizeOfArray(data, ref offset);
            data = data[offset..];
            int arraySizeIncrementor = 0;
            for (int i = 0; i < arraySize; i++)
            {
                LoginSuccessPacketElement tmp = new();
                int tmpSize = tmp.decodeFromBytes(data);
                arraySizeIncrementor += tmpSize;
                data = data[tmpSize..];
                _Elements.Add(tmp);
            }
            return 16 + offset + arraySizeIncrementor;
        }
    }

    namespace Internals
    {
        public class LoginSuccessPacketElement
        {
            public string s1 = "";
            public string s2 = "";
            public string? optionalS3;

            public int decodeFromBytes(byte[] data)
            {
                int offset = 0;
                s1 = StringN.DecodeBytes(data, ref offset);
                s2 = StringN.DecodeBytes(data, ref offset);
                bool isPresent = PrefixedOptional.DecodeBytes(data, ref offset);
                if (!isPresent)
                {
                    return offset;
                }
                optionalS3 = StringN.DecodeBytes(data, ref offset);
                return offset;
            }
        }
    }
}
