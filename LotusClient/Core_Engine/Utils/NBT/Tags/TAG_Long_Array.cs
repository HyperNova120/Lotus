using Core_Engine.BaseClasses.Types;
using Core_Engine.Utils.NBT.BaseClasses;

namespace Core_Engine.Utils.NBT.Tags;

public class TAG_Long_Array : TAG_Base
{
    public long[] Values;

    public TAG_Long_Array()
    {
        Values = [];
        Type_ID = 12;
    }

    public override byte[] ProcessBytes(byte[] inputBytes)
    {
        inputBytes = ProcessIDAndNameBytes(inputBytes);

        int sizeOfValue = sizeof(long);

        (int arraySize, int numBytesRead) = PrefixedArray.GetSizeOfArray(inputBytes);
        Values = [arraySize];
        for (int i = 0; i < arraySize; i++)
        {
            int startIndex = numBytesRead + (sizeOfValue * i);
            Values[i] = BitConverter.ToInt64(
                inputBytes[numBytesRead..(numBytesRead + sizeOfValue)].Reverse().ToArray(),
                0
            );
        }

        return inputBytes[(numBytesRead + (sizeOfValue * arraySize))..];
    }

    public override string ToString(int tabSpace = 0)
    {
        string returner =
            new string('\t', tabSpace)
            + $"TAG_Long_Array({((Name.Length == 0) ? "None" : "\'" + Name + "\'")}):";
        foreach (long cur in Values)
        {
            returner += " " + cur.ToString();
        }
        return returner;
    }
}
