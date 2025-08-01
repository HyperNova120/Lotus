using Core_Engine.Utils.NBT.BaseClasses;

namespace Core_Engine.Utils.NBT.Tags;

public class TAG_Int : TAG_Base
{
    public int Value;

    public TAG_Int()
    {
        Type_ID = 3;
    }

    public override byte[] ProcessBytes(byte[] inputBytes)
    {
        inputBytes = ProcessIDAndNameBytes(inputBytes);

        //payload
        Value = BitConverter.ToInt32(inputBytes[0..4].Reverse().ToArray(), 0);
        //return remaining bytes

        return inputBytes[4..];
    }

    public override string ToString(int tabSpace = 0)
    {
        return new string('\t', tabSpace)
            + $"TAG_Int({((Name.Length == 0) ? "None" : "\'" + Name + "\'")}): {Value}";
    }
}
