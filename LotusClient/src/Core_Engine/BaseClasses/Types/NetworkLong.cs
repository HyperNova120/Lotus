using LotusCore.Interfaces;

namespace LotusCore.BaseClasses.Types
{
    public class NetworkLong : INetworkData<long>
    {
        public static byte[] GetBytes(long data)
        {
            return [.. BitConverter.GetBytes(data).Reverse()];
        }

        public static long DecodeBytes(byte[] data, ref int offset)
        {
            long returner = BitConverter.ToInt64(
                data[offset..(offset + sizeof(long))].Reverse().ToArray()
            );
            offset += sizeof(long);
            return returner;
        }
    }
}
