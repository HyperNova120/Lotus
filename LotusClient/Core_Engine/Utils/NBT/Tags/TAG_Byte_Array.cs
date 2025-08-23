using LotusCore.BaseClasses.Types;
using LotusCore.Exceptions;
using LotusCore.Utils.NBTInternals.BaseClasses;
using Microsoft.AspNetCore.Mvc.Diagnostics;

namespace LotusCore.Utils.NBTInternals.Tags;

public class TAG_Byte_Array : TAG_Base
{
    public sbyte[] Values;

    public TAG_Byte_Array()
    {
        Values = [];
        _Type_ID = 7;
    }

    public override TAG_Base Clone()
    {
        TAG_Byte_Array ret = new();
        ret._IsInListTag = _IsInListTag;
        ret._Name = _Name;
        ret.Values = (sbyte[])Values.Clone();
        return ret;
    }

    public override byte[] GetBytes()
    {
        return
        [
            .. GetIDAndNamesBytes(),
            .. BitConverter.GetBytes(Values.Length).Reverse(),
            .. (byte[])(Array)Values,
        ];
    }

    public override int ProcessBytes(byte[] inputBytes)
    {
        int offset = ProcessIDAndNameBytes(inputBytes);

        //payload
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

        Values = (sbyte[])
            (Array)inputBytes[(offset + numBytesRead)..(offset + numBytesRead + arraySize)];
        //return remaining bytes

        return offset + numBytesRead + arraySize;
    }

    public override string ToString(int tabSpace = 0)
    {
        string returner =
            new string('\t', tabSpace)
            + $"TAG_Byte_Array({((_Name.Length == 0) ? "None" : "\'" + _Name + "\'")}):";
        foreach (sbyte cur in Values)
        {
            returner += " " + ((byte)cur).ToString("X2");
        }
        return returner;
    }
}
