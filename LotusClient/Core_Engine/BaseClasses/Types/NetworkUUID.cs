using LotusCore.Interfaces;

namespace LotusCore.BaseClasses.Types
{
    public class NetworkUUID
    {
        public static byte[] GetNetworkBytes(UInt128 uuid)
        {
            Int64 lowBits = (long)(uuid & 0xFFFFFFFFFFFFFFFF);
            Int64 highBites = (long)((uuid >> 64) & 0xFFFFFFFFFFFFFFFF);
            return [.. BitConverter.GetBytes(highBites), .. BitConverter.GetBytes(lowBits)];
        }

        public static (byte[] value, int numBytesRead) DecodeNetworkBytes(byte[] data)
        {
            byte[] highBytes = data[..8];
            byte[] lowBytes = data[8..16];
            return ([.. highBytes, .. lowBytes], 16);
        }
    }
}
