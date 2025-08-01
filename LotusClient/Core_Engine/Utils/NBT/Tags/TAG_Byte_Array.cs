using Core_Engine.BaseClasses.Types;
using Core_Engine.Exceptions;
using Core_Engine.Utils.NBT.BaseClasses;

namespace Core_Engine.Utils.NBT.Tags;

public class TAG_Byte_Array : TAG_Base
{
    public sbyte[] Values;

    public TAG_Byte_Array()
    {
        Values = [];
        Type_ID = 7;
    }

    public override byte[] ProcessBytes(byte[] inputBytes)
    {
        inputBytes = ProcessIDAndNameBytes(inputBytes);

        //payload
        int numBytesRead = 4;
        int arraySize = BitConverter.ToInt32(inputBytes[..4].Reverse().ToArray(), 0);

        Values = (sbyte[])(Array)inputBytes[numBytesRead..(numBytesRead + arraySize)];
        //return remaining bytes

        return inputBytes[(numBytesRead + arraySize)..];
    }

    public override string ToString(int tabSpace = 0)
    {
        string returner =
            new string('\t', tabSpace)
            + $"TAG_Byte_Array({((Name.Length == 0) ? "None" : "\'" + Name + "\'")}):";
        foreach (sbyte cur in Values)
        {
            returner += " " + ((byte)cur).ToString("X2");
        }
        return returner;
    }
}
