using LotusCore.Interfaces;

namespace LotusCore.BaseClasses.Types
{
    public class NetworkInt : INetworkData<int>
    {
        public static byte[] GetBytes(int data)
        {
            return [.. BitConverter.GetBytes(data).Reverse()];
        }

        public static int DecodeBytes(byte[] data, ref int offset)
        {
            int returner = BitConverter.ToInt32(
                data[offset..(offset + sizeof(int))].Reverse().ToArray()
            );
            offset += sizeof(int);
            return returner;
        }
    }
}
