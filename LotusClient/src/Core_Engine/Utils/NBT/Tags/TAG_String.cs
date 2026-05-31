using System.Text;
using LotusCore.Utils.NBTInternals.BaseClasses;

namespace LotusCore.Utils.NBTInternals.Tags;

public class TAG_String : TAG_Base
{
    public string Value = "";

    public TAG_String()
    {
        _Type_ID = 8;
    }

    public override TAG_Base Clone()
    {
        TAG_String ret = new();
        ret._IsInListTag = _IsInListTag;
        ret._Name = _Name;
        ret.Value = Value;
        return ret;
    }

    public override byte[] GetBytes()
    {
        byte[] valueBytes = Encoding.UTF8.GetBytes(Value.Replace("\n", "").Replace("\r", ""));
        return
        [
            .. GetIDAndNamesBytes(),
            (byte)((valueBytes.Length & 0xFF00) >> 8),
            (byte)(valueBytes.Length & 0xFF),
            .. valueBytes,
        ];
    }

    public override int ProcessBytes(byte[] inputBytes)
    {
        int offset = ProcessIDAndNameBytes(inputBytes);

        ushort length = inputBytes[offset + 0];
        length <<= 8;
        length |= inputBytes[offset + 1];

        /* for (int i = 0; i < length; i++)
        {
            Value += (char)inputBytes[offset + 2 + i];
        } */
        Value = Encoding
            .UTF8.GetString(inputBytes[(offset + 2)..(offset + 2 + length)])
            .Replace("\r", "")
            .Replace("\n", "");

        return offset + 2 + length;
    }

    public override string ToString(int tabSpace = 0)
    {
        //string returnValue = Encoding.UTF8.GetString(Encoding.Default.GetBytes(Value));
        return new string('\t', tabSpace)
            + $"TAG_String({((_Name.Length == 0) ? "None" : "\'" + _Name + "\'")}): \'{Value}\'";
    }
}
