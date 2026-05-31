using LotusCore.Interfaces;

namespace LotusCore.BaseClasses.Types
{
    public class PrefixedArray : INetworkData<byte[]>
    {
        public static byte[] GetBytes(byte[] data)
        {
            return [.. VarInt_VarLong.EncodeInt(data.Length), .. data];
        }

        public static byte[] GetBytes(int numElements, byte[] totalData)
        {
            return [.. VarInt_VarLong.EncodeInt(numElements), .. totalData];
        }

        public static (byte[] bytesOfArray, int numberBytesRead) DecodeBytes(byte[] data)
        {
            int offset = 0;
            int size = VarInt_VarLong.DecodeVarInt(data, ref offset);

            byte[] arrayBytes = (size == 0) ? [] : data[offset..(offset + size)];
            /* Logging.LogDebug(
                $"PrefixedArray Array_Length:{size} Tota_Byte_Length:{numSizeBytes + arrayBytes.Length}"
            ); */
            return (arrayBytes, offset + arrayBytes.Length);
        }

        public static byte[] DecodeBytes(byte[] data, ref int offset)
        {
            int size = VarInt_VarLong.DecodeVarInt(data, ref offset);
            byte[] arrayBytes = (size == 0) ? [] : data[offset..(offset + size)];
            offset += size;
            return arrayBytes;
        }

        public static int GetSizeOfArray(byte[] data, ref int offset)
        {
            return VarInt_VarLong.DecodeVarInt(data, ref offset);
        }
    }
}
