namespace Core_Engine.BaseClasses.Types
{
    public static class NetworkDouble
    {
        public static byte[] GetBytes(double data)
        {
            return [.. BitConverter.GetBytes(data).Reverse()];
        }

        public static (double value, int numBytesRead) DecodeBytes(byte[] data)
        {
            return (
                BitConverter.ToDouble(data[..sizeof(double)].Reverse().ToArray()),
                sizeof(double)
            );
        }
    }
}
