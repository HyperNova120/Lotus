using Core_Engine.Utils.NBT.BaseClasses;

namespace Core_Engine.Utils.NBT.Tags;

public class TAG_Short : TAG_Base
{
    public short Value;

    public TAG_Short()
    {
        Type_ID = 2;
    }

    public override byte[] ProcessBytes(byte[] inputBytes)
    {
        inputBytes = ProcessIDAndNameBytes(inputBytes);

        //payload
        Value = BitConverter.ToInt16([inputBytes[1], inputBytes[0]], 0);
        //return remaining bytes

        return inputBytes[2..];
    }

    public override string ToString(int tabSpace = 0)
    {
        return new string('\t', tabSpace)
            + $"TAG_Short({((Name.Length == 0) ? "None" : "\'" + Name + "\'")}): {Value}";
    }
}
