namespace Core_Engine.BaseClasses.Types
{
    public static class NetworkInt
    {
        public static byte[] GetBytes(int data)
        {
            return [.. BitConverter.GetBytes(data).Reverse()];
        }

        public static int DecodeBytes(byte[] data)
        {
            return BitConverter.ToInt32(data[..4].Reverse().ToArray());
        }
    }
}
