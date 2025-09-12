using LotusCore.Interfaces;

namespace LotusCore.BaseClasses.Types
{
    public class NetworkShort : INetworkData<short>
    {
        public static byte[] GetBytes(short data)
        {
            return [.. BitConverter.GetBytes(data).Reverse()];
        }

        public static short DecodeBytes(byte[] data, ref int offset)
        {
            short returner = BitConverter.ToInt16(
                data[offset..(offset + sizeof(short))].Reverse().ToArray()
            );
            offset += sizeof(short);
            return returner;
        }
    }
}
