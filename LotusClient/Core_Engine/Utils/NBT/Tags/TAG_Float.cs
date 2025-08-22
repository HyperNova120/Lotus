using Core_Engine.Exceptions;
using Core_Engine.Utils.NBTInternals.BaseClasses;

namespace Core_Engine.Utils.NBTInternals.Tags;

public class TAG_Float : TAG_Base
{
    public float Value;

    public TAG_Float()
    {
        _Type_ID = 5;
    }

    public override TAG_Base Clone()
    {
        TAG_Float ret = new();
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
        //Value = BitConverter.ToSingle(inputBytes[0..4].Reverse().ToArray(), 0);
        Value = BitConverter.ToSingle(
            [
                inputBytes[offset + 3],
                inputBytes[offset + 2],
                inputBytes[offset + 1],
                inputBytes[offset + 0],
            ],
            0
        );
        //return remaining bytes

        return offset + 4;
    }

    public override string ToString(int tabSpace = 0)
    {
        return new string('\t', tabSpace)
            + $"TAG_Float({((_Name.Length == 0) ? "None" : "\'" + _Name + "\'")}): {Value}";
    }
}
