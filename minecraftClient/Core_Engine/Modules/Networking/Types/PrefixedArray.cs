
namespace Core_Engine.Modules.Networking.Types
{
    public static class PrefixedArray
    {
        public static byte[] GetBytes(byte[] data)
        {
            return [.. VarInt_VarLong.EncodeInt(data.Length), .. data];
        }

        public static (byte[] bytes, int numberBytesRead) DecodeBytes(byte[] data)
        {
            (int value, int dataBytes) = VarInt_VarLong.DecodeVarInt(data);
            var arrayBytes = new Span<byte>(data, dataBytes, value);
            byte[] bytes = (dataBytes == 0) ? [] : arrayBytes.ToArray();
            Logging.LogDebug(
                $"PrefixedArray Array_Length:{value} Tota_Byte_Length:{dataBytes + bytes.Length}"
            );
            return (bytes, dataBytes + bytes.Length);
        }
    }
}
