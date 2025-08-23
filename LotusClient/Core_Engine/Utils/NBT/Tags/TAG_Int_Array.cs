using LotusCore.BaseClasses.Types;
using LotusCore.Utils.NBTInternals.BaseClasses;

namespace LotusCore.Utils.NBTInternals.Tags;

public class TAG_Int_Array : TAG_Base
{
    public int[] Values;

    public TAG_Int_Array()
    {
        Values = [];
        _Type_ID = 11;
    }

    public override TAG_Base Clone()
    {
        TAG_Int_Array ret = new();
        ret._IsInListTag = _IsInListTag;
        ret._Name = _Name;
        ret.Values = (int[])Values.Clone();
        return ret;
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

        Values = new int[arraySize];

        offset += 4;
        int i = 0;
        try
        {
            for (i = 0; i < arraySize; i++)
            {
                Values[i] = BitConverter.ToInt32(
                    [
                        inputBytes[offset + 3],
                        inputBytes[offset + 2],
                        inputBytes[offset + 1],
                        inputBytes[offset],
                    ],
                    0
                );
                offset += 4;
            }
        }
        catch (Exception e)
        {
            Logging.LogError(
                $"INT_ARRAY_OFFSET:{offset} inputBytes.length:{inputBytes.Length} Values.length:{Values.Length} i:{i}"
            );
            throw;
        }

        return offset;
    }

    public override string ToString(int tabSpace = 0)
    {
        string returner =
            new string('\t', tabSpace)
            + $"TAG_Int_Array({((_Name.Length == 0) ? "None" : "\'" + _Name + "\'")}):";
        foreach (int cur in Values)
        {
            returner += " " + cur.ToString();
        }
        return returner;
    }
}
