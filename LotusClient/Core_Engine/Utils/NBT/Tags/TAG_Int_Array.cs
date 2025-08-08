using Core_Engine.BaseClasses.Types;
using Core_Engine.Utils.NBTInternals.BaseClasses;

namespace Core_Engine.Utils.NBTInternals.Tags;

public class TAG_Int_Array : TAG_Base
{
    public int[] Values;

    public TAG_Int_Array()
    {
        Values = [];
        Type_ID = 11;
    }

    public override byte[] GetBytes()
    {
        byte[] ValuesBytes = new byte[sizeof(int) * Values.Length];
        for (int i = 0; i < Values.Length; i++)
        {
            int baseIndex = i * sizeof(int);
            byte[] tmp = [.. BitConverter.GetBytes(Values[i]).Reverse()];
            for (int j = 0; j < sizeof(int); j++)
            {
                ValuesBytes[baseIndex + j] = tmp[j];
            }
        }
        return
        [
            .. GetIDAndNamesBytes(),
            .. BitConverter.GetBytes(Values.Length).Reverse(),
            .. ValuesBytes,
        ];
    }

    public override byte[] ProcessBytes(byte[] inputBytes)
    {
        int offset = ProcessIDAndNameBytes(inputBytes);

        int sizeOfValue = sizeof(int);

        int numBytesRead = 4;
        //int arraySize = BitConverter.ToInt32(inputBytes[..4].Reverse().ToArray(), 0);
        int arraySize = BitConverter.ToInt32(
            [
                inputBytes[offset + 3],
                inputBytes[offset + 2],
                inputBytes[offset + 1],
                inputBytes[offset + 0],
            ],
            0
        );

        Values = [arraySize];

        int startIndex;
        for (int i = 0; i < arraySize; i++)
        {
            startIndex = offset + numBytesRead + (sizeOfValue * i);
            /* Values[i] = BitConverter.ToInt32(
                inputBytes[startIndex..(startIndex + sizeOfValue)].Reverse().ToArray(),
                0
            ); */
            Values[i] = BitConverter.ToInt32(
                [
                    inputBytes[startIndex + 3],
                    inputBytes[startIndex + 2],
                    inputBytes[startIndex + 1],
                    inputBytes[startIndex],
                ],
                0
            );
        }

        return inputBytes[(offset + numBytesRead + (sizeOfValue * arraySize))..];
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
