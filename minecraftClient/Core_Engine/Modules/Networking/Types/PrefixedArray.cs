namespace Core_Engine.Modules.Networking.Types
{
    public static class PrefixedArray
    {
        public static byte[] GetBytes(byte[] data)
        {
            return [.. VarInt_VarLong.EncodeInt(data.Length), .. data];
        }

        public static (byte[] bytesOfArray, int numberBytesRead) DecodeBytes(byte[] data)
        {
            (int size, int numSizeBytes) = VarInt_VarLong.DecodeVarInt(data);
            data = data[numSizeBytes..];

            byte[] arrayBytes = (size == 0) ? [] : data[..size];
            Logging.LogDebug(
                $"PrefixedArray Array_Length:{size} Tota_Byte_Length:{numSizeBytes + arrayBytes.Length}"
            );
            return (arrayBytes, numSizeBytes + arrayBytes.Length);
        }

        public static (int arraySize, int numBytesRead) GetSizeOfArray(byte[] data)
        {
            (int size, int numSizeBytes) = VarInt_VarLong.DecodeVarInt(data);
            return (size, numSizeBytes);
        }
    }
}
