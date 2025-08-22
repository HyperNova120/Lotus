using Core_Engine.Utils.NBTInternals.BaseClasses;

namespace Core_Engine.Utils.NBTInternals.Tags;

public class TAG_Short : TAG_Base
{
    public short Value;

    public TAG_Short()
    {
        _Type_ID = 2;
    }

    public override TAG_Base Clone()
    {
        TAG_Short ret = new();
        ret._IsInListTag = _IsInListTag;
        ret._Name = _Name;
        ret.Value = Value;
        return ret;
    }

    public override byte[] GetBytes()
    {
        return [.. GetIDAndNamesBytes(), .. BitConverter.GetBytes(Value).Reverse()];
    }

    public override int ProcessBytes(byte[] inputBytes)
    {
        int offset = ProcessIDAndNameBytes(inputBytes);

        //payload
        Value = BitConverter.ToInt16([inputBytes[offset + 1], inputBytes[offset + 0]], 0);
        //return remaining bytes

        return offset + 2;
    }

    public override string ToString(int tabSpace = 0)
    {
        return new string('\t', tabSpace)
            + $"TAG_Short({((_Name.Length == 0) ? "None" : "\'" + _Name + "\'")}): {Value}";
    }
}
