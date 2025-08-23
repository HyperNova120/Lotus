namespace LotusCore.BaseClasses.Types
{
    public static class NetworkLong
    {
        public static byte[] GetBytes(long data)
        {
            return [.. BitConverter.GetBytes(data).Reverse()];
        }

        public static long DecodeBytes(byte[] data)
        {
            return BitConverter.ToInt64(data[..8].Reverse().ToArray());
        }
    }
}
