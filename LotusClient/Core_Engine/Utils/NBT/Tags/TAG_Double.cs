using Core_Engine.Exceptions;
using Core_Engine.Utils.NBTInternals.BaseClasses;

namespace Core_Engine.Utils.NBTInternals.Tags;

public class TAG_Double : TAG_Base
{
    public double Value;

    public TAG_Double()
    {
        Type_ID = 6;
    }

    public override byte[] GetBytes()
    {
        return [.. GetIDAndNamesBytes(), .. BitConverter.GetBytes(Value).Reverse()];
    }

    public override int ProcessBytes(byte[] inputBytes)
    {
        int offset = ProcessIDAndNameBytes(inputBytes);

        //payload
        //Value = BitConverter.ToDouble(inputBytes[0..8].Reverse().ToArray(), 0);
        Value = BitConverter.ToDouble(
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

        return offset + 8;
    }

    public override string ToString(int tabSpace = 0)
    {
        return new string('\t', tabSpace)
            + $"TAG_Double({((Name.Length == 0) ? "None" : "\'" + Name + "\'")}): {Value}";
    }
}
