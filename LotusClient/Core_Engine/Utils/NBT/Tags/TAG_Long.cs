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
        Value = BitConverter.ToInt64(
            [
                inputBytes[7],
                inputBytes[6],
                inputBytes[5],
                inputBytes[4],
                inputBytes[3],
                inputBytes[2],
                inputBytes[1],
                inputBytes[0],
            ],
            0
        );
        //return remaining bytes

        return inputBytes[8..];
    }

    public override string ToString(int tabSpace = 0)
    {
        return new string('\t', tabSpace)
            + $"TAG_Long({((Name.Length == 0) ? "None" : "\'" + Name + "\'")}): {Value}";
    }
}
