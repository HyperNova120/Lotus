using LotusCore.BaseClasses.Types;
using LotusCore.Utils.NBTInternals.BaseClasses;

namespace LotusCore.Utils.NBTInternals.Tags;

public class TAG_Long_Array : TAG_Base
{
    public long[] Values;

    public TAG_Long_Array()
    {
        Values = [];
        _Type_ID = 12;
    }

    public override TAG_Base Clone()
    {
        TAG_Long_Array ret = new();
        ret._IsInListTag = _IsInListTag;
        ret._Name = _Name;
        ret.Values = (long[])Values.Clone();
        return ret;
    }

    public override byte[] GetBytes()
    {
        byte[] ValuesBytes = new byte[sizeof(long) * Values.Length];
        for (int i = 0; i < Values.Length; i++)
        {
            int baseIndex = i * sizeof(long);
            byte[] tmp = [.. BitConverter.GetBytes(Values[i]).Reverse()];
            for (int j = 0; j < sizeof(long); j++)
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

    public override int ProcessBytes(byte[] inputBytes)
    {
        int offset = ProcessIDAndNameBytes(inputBytes);
        int arraySize = BitConverter.ToInt32(
            [
                inputBytes[offset + 3],
                inputBytes[offset + 2],
                inputBytes[offset + 1],
                inputBytes[offset],
            ],
            0
        );

        Values = new long[arraySize];

        offset += 4;
        for (int i = 0; i < arraySize; i++)
        {
            Values[i] = BitConverter.ToInt64(
                [
                    inputBytes[offset + 7],
                    inputBytes[offset + 6],
                    inputBytes[offset + 5],
                    inputBytes[offset + 4],
                    inputBytes[offset + 3],
                    inputBytes[offset + 2],
                    inputBytes[offset + 1],
                    inputBytes[offset],
                ],
                0
            );
            offset += 8;
        }

        return offset;
    }

    public override string ToString(int tabSpace = 0)
    {
        string returner =
            new string('\t', tabSpace)
            + $"TAG_Long_Array({((_Name.Length == 0) ? "None" : "\'" + _Name + "\'")}):";
        foreach (long cur in Values)
        {
            returner += " " + cur.ToString();
        }
        return returner;
    }
}
