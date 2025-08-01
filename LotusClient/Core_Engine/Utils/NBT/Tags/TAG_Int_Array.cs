using Core_Engine.BaseClasses.Types;
using Core_Engine.Utils.NBT.BaseClasses;

namespace Core_Engine.Utils.NBT.Tags;

public class TAG_Int_Array : TAG_Base
{
    public int[] Values;

    public TAG_Int_Array()
    {
        Values = [];
        Type_ID = 11;
    }

    public override byte[] ProcessBytes(byte[] inputBytes)
    {
        inputBytes = ProcessIDAndNameBytes(inputBytes);

        int sizeOfValue = sizeof(int);

        (int arraySize, int numBytesRead) = PrefixedArray.GetSizeOfArray(inputBytes);
        Values = [arraySize];
        for (int i = 0; i < arraySize; i++)
        {
            int startIndex = numBytesRead + (sizeOfValue * i);
            Values[i] = BitConverter.ToInt32(
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
            + $"TAG_Int_Array({((Name.Length == 0) ? "None" : "\'" + Name + "\'")}):";
        foreach (int cur in Values)
        {
            returner += " " + cur.ToString();
        }
        return returner;
    }
}
