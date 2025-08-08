using Core_Engine.Utils.NBTInternals.BaseClasses;

namespace Core_Engine.Utils.NBTInternals.Tags;

public class TAG_Int : TAG_Base
{
    public int Value;

    public TAG_Int()
    {
        Type_ID = 3;
    }

    public override byte[] GetBytes()
    {
        return [.. GetIDAndNamesBytes(), .. BitConverter.GetBytes(Value).Reverse()];
    }

    public override byte[] ProcessBytes(byte[] inputBytes)
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

        return inputBytes[(offset + 4)..];
    }

    public override string ToString(int tabSpace = 0)
    {
        return new string('\t', tabSpace)
            + $"TAG_Int({((Name.Length == 0) ? "None" : "\'" + Name + "\'")}): {Value}";
    }
}
