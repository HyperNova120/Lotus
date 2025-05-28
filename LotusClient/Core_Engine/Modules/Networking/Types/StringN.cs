using System.Text;

namespace Core_Engine.Modules.Networking.Types
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
            string strValue =
                (size > 0) ? Encoding.UTF8.GetString(bytes[numBytes..(numBytes + size)]) : "";
            return (strValue, strValue.Length + numBytes);
        }
    }
}
