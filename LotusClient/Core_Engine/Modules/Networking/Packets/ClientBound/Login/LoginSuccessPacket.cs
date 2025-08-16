using Core_Engine.Modules.Networking.Packets.ClientBound.Login.Internals;
using Core_Engine.BaseClasses.Types;

namespace Core_Engine.Modules.Networking.Packets.ClientBound.Login
{
    public class LoginSuccessPacket
    {
        public Guid uuid;
        public string? Username;
        public List<LoginSuccessPacketElement> elements = new();

        /// <summary>
        /// populates packet based on data
        /// </summary>
        /// <param name="data"></param>
        /// <returns>number of bytes read</returns>
        public int DecodeFromBytes(byte[] data)
        {
            (byte[] uuidBytes, _) = NetworkUUID.DecodeNetworkBytes(data[0..16]);
            uuid = new Guid(uuidBytes);
            data = data[16..];
            (Username, int usernameNumBytes) = StringN.DecodeBytes(data);
            data = data[usernameNumBytes..];
            (int arraySize, int arraySizeNumBytesRead) = PrefixedArray.GetSizeOfArray(data);
            data = data[arraySizeNumBytesRead..];
            int arraySizeIncrementor = 0;
            for (int i = 0; i < arraySize; i++)
            {
                LoginSuccessPacketElement tmp = new();
                int tmpSize = tmp.decodeFromBytes(data);
                arraySizeIncrementor += tmpSize;
                data = data[tmpSize..];
                elements.Add(tmp);
            }

            return 16 + usernameNumBytes + arraySizeNumBytesRead + arraySizeIncrementor;
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
                (s1, int s1NumBytes) = StringN.DecodeBytes(data);
                data = data[s1NumBytes..];
                (s2, int s2NumBytes) = StringN.DecodeBytes(data);
                data = data[s2NumBytes..];
                (bool isPresent, int isPresentNumBytes) = PrefixedOptional.DecodeBytes(data);
                if (!isPresent)
                {
                    return s1NumBytes + s2NumBytes + isPresentNumBytes;
                }
                data = data[1..];
                (optionalS3, int seNumBytes) = StringN.DecodeBytes(data);
                return s1NumBytes + s2NumBytes + isPresentNumBytes + seNumBytes;
            }
        }
    }
}
