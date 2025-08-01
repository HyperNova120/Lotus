using Core_Engine.Exceptions;
using Core_Engine.Utils.NBT.BaseClasses;

namespace Core_Engine.Utils.NBT.Tags;

public class TAG_Float : TAG_Base
{
    public float Value;

    public TAG_Float()
    {
        Type_ID = 5;
    }

    public override byte[] ProcessBytes(byte[] inputBytes)
    {
        inputBytes = ProcessIDAndNameBytes(inputBytes);

        //payload
        //Value = BitConverter.ToSingle(inputBytes[0..4].Reverse().ToArray(), 0);
        Value = BitConverter.ToSingle(
            [inputBytes[3], inputBytes[2], inputBytes[1], inputBytes[0]],
            0
        );
        //return remaining bytes

        return inputBytes[4..];
    }

    public override string ToString(int tabSpace = 0)
    {
        return new string('\t', tabSpace)
            + $"TAG_Float({((Name.Length == 0) ? "None" : "\'" + Name + "\'")}): {Value}";
    }
}
