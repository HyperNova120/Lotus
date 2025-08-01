using Core_Engine.Utils.NBT.BaseClasses;

namespace Core_Engine.Utils.NBT.Tags;

public class TAG_Long : TAG_Base
{
    public long Value;

    public TAG_Long()
    {
        Type_ID = 4;
    }

    public override byte[] ProcessBytes(byte[] inputBytes)
    {
        inputBytes = ProcessIDAndNameBytes(inputBytes);

        //payload
        Value = BitConverter.ToInt64(inputBytes[0..8].Reverse().ToArray(), 0);
        //return remaining bytes

        return inputBytes[8..];
    }

    public override string ToString(int tabSpace = 0)
    {
        return new string('\t', tabSpace)
            + $"TAG_Long({((Name.Length == 0) ? "None" : "\'" + Name + "\'")}): {Value}";
    }
}
