using Core_Engine.Exceptions;
using Core_Engine.Utils.NBTInternals.BaseClasses;

namespace Core_Engine.Utils.NBTInternals.Tags;

public class TAG_Byte : TAG_Base
{
    public sbyte Value;

    public TAG_Byte()
    {
        _Type_ID = 1;
    }

    public override TAG_Base Clone()
    {
        TAG_Byte ret = new();
        ret._IsInListTag = _IsInListTag;
        ret._Name = _Name;
        ret.Value = Value;
        return ret;
    }

    public override byte[] GetBytes()
    {
        return [.. GetIDAndNamesBytes(), (byte)Value];
    }

    public override int ProcessBytes(byte[] inputBytes)
    {
        int offset = ProcessIDAndNameBytes(inputBytes);

        //payload
        Value = (sbyte)inputBytes[offset + 0];

        //return remaining bytes
        return offset + 1;
    }

    public override string ToString(int tabSpace = 0)
    {
        return new string('\t', tabSpace)
            + $"TAG_Byte({((_Name.Length == 0) ? "None" : "\'" + _Name + "\'")}): {Value}";
    }
}
