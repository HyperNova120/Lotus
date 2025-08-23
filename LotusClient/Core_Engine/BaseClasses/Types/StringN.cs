using System.Text;

namespace LotusCore.BaseClasses.Types
{
    public static class StringN
    {
        public static byte[] GetBytes(string value)
        {
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            return [.. VarInt_VarLong.EncodeInt(value.Length), .. valueBytes];
        }

        public static (string value, int numBytes) DecodeBytes(byte[] bytes)
        {
            (int size, int numBytes) = VarInt_VarLong.DecodeVarInt(bytes);
            string strValue = Encoding.UTF8.GetString(bytes[numBytes..(numBytes + size)]);
            return (strValue, size + numBytes);
        }
    }
}
