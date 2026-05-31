using System.Text;
using LotusCore.Interfaces;

namespace LotusCore.BaseClasses.Types
{
    public class StringN : INetworkData<string>
    {
        public static byte[] GetBytes(string value)
        {
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            return [.. VarInt_VarLong.EncodeInt(value.Length), .. valueBytes];
        }

        public static string DecodeBytes(byte[] bytes, ref int offset)
        {
            int size = VarInt_VarLong.DecodeVarInt(bytes, ref offset);
            string strValue = Encoding.UTF8.GetString(bytes[offset..(offset + size)]);
            offset += size;
            return strValue;
        }
    }
}
