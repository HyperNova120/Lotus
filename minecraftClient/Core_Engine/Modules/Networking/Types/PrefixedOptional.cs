namespace Core_Engine.Modules.Networking.Types
{
    public static class PrefixedOptional
    {
        public static byte[] GetBytes(byte[] data)
        {
            return [(byte)((data.Length == 0) ? 0x00 : 0x01), .. data];
        }

        public static (bool isPresent, int numberBytesRead) DecodeBytes(byte[] data)
        {
            return (data[0] == 0x01, 1);
        }
    }
}
