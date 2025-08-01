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

        int numBytesRead = 4;
        //int arraySize = BitConverter.ToInt32(inputBytes[..4].Reverse().ToArray(), 0);
        int arraySize = BitConverter.ToInt32(
            [inputBytes[3], inputBytes[2], inputBytes[1], inputBytes[0]],
            0
        );

        Values = [arraySize];
        int startIndex;
        for (int i = 0; i < arraySize; i++)
        {
            startIndex = numBytesRead + (sizeOfValue * i);
            /* Values[i] = BitConverter.ToInt64(
                inputBytes[startIndex..(startIndex + sizeOfValue)].Reverse().ToArray(),
                0
            ); */
            Values[i] = BitConverter.ToInt64(
                [
                    inputBytes[startIndex + 3],
                    inputBytes[startIndex + 2],
                    inputBytes[startIndex + 1],
                    inputBytes[startIndex],
                ],
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
