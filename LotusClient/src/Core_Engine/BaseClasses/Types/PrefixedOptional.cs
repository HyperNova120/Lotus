using LotusCore.Interfaces;

namespace LotusCore.BaseClasses.Types
{
    public class PrefixedOptional : INetworkData<bool, byte[]>
    {
        public static byte[] GetBytes(byte[] data)
        {
            return [(byte)((data.Length == 0) ? 0x00 : 0x01), .. data];
        }

        public static bool DecodeBytes(byte[] data, ref int offset)
        {
            return data[offset++] == 0x01;
        }
    }
}
