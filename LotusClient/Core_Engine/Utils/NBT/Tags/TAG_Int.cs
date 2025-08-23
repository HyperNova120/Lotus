using LotusCore.Utils.NBTInternals.BaseClasses;

namespace LotusCore.Utils.NBTInternals.Tags;

public class TAG_Int : TAG_Base
{
    public int Value;

    public TAG_Int()
    {
        _Type_ID = 3;
    }

    public override TAG_Base Clone()
    {
        TAG_Int ret = new();
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
        //Value = BitConverter.ToInt32(inputBytes[0..4].Reverse().ToArray(), 0);
        Value = BitConverter.ToInt32(
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
            + $"TAG_Int({((_Name.Length == 0) ? "None" : "\'" + _Name + "\'")}): {Value}";
    }
}
