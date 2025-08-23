namespace LotusCore.BaseClasses.Types
{
    public static class NetworkShort
    {
        public static byte[] GetBytes(short data)
        {
            return [.. BitConverter.GetBytes(data).Reverse()];
        }

        public static (short value, int numBytesRead) DecodeBytes(byte[] data)
        {
            return (BitConverter.ToInt16(data[..2].Reverse().ToArray()), 2);
        }
    }
}
