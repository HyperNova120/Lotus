using LotusCore.Interfaces;

namespace LotusCore.BaseClasses.Types
{
    public class NetworkDouble : INetworkData<double>
    {
        public static byte[] GetBytes(double data)
        {
            return [.. BitConverter.GetBytes(data).Reverse()];
        }

        public static double DecodeBytes(byte[] data, ref int offset)
        {
            double returner = BitConverter.ToDouble(
                data[offset..(offset + sizeof(double))].Reverse().ToArray()
            );
            offset += sizeof(double);
            return returner;
        }
    }
}
