using Core_Engine.Exceptions;
using Core_Engine.Utils.NBT.BaseClasses;

namespace Core_Engine.Utils.NBT.Tags;

public class TAG_Double : TAG_Base
{
    public double Value;

    public TAG_Double()
    {
        Type_ID = 6;
    }

    public override byte[] ProcessBytes(byte[] inputBytes)
    {
        inputBytes = ProcessIDAndNameBytes(inputBytes);

        //payload
        //Value = BitConverter.ToDouble(inputBytes[0..8].Reverse().ToArray(), 0);
        Value = BitConverter.ToDouble(
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
            + $"TAG_Double({((Name.Length == 0) ? "None" : "\'" + Name + "\'")}): {Value}";
    }
}
