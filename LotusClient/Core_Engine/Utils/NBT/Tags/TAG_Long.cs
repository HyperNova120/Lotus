using Core_Engine.Utils.NBTInternals.BaseClasses;

namespace Core_Engine.Utils.NBTInternals.Tags;

public class TAG_Long : TAG_Base
{
    public long Value;

    public TAG_Long()
    {
        Type_ID = 4;
    }

    public override byte[] GetBytes()
    {
        return [.. GetIDAndNamesBytes(), .. BitConverter.GetBytes(Value).Reverse()];
    }

    public override byte[] ProcessBytes(byte[] inputBytes)
    {
        int offset = ProcessIDAndNameBytes(inputBytes);

        //payload
        Value = BitConverter.ToInt64(
            [
                inputBytes[offset + 7],
                inputBytes[offset + 6],
                inputBytes[offset + 5],
                inputBytes[offset + 4],
                inputBytes[offset + 3],
                inputBytes[offset + 2],
                inputBytes[offset + 1],
                inputBytes[offset + 0],
            ],
            0
        );
        //return remaining bytes

        return inputBytes[(offset + 8)..];
    }

    public override string ToString(int tabSpace = 0)
    {
        return new string('\t', tabSpace)
            + $"TAG_Long({((Name.Length == 0) ? "None" : "\'" + Name + "\'")}): {Value}";
    }
}
