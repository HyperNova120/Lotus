using Core_Engine.Exceptions;
using Core_Engine.Utils.NBT.BaseClasses;

namespace Core_Engine.Utils.NBT.Tags;

public class TAG_Byte : TAG_Base
{
    public sbyte Value;

    public TAG_Byte()
    {
        Type_ID = 1;
    }

    public override byte[] ProcessBytes(byte[] inputBytes)
    {
        int offset = ProcessIDAndNameBytes(inputBytes);

        //payload
        Value = (sbyte)inputBytes[offset + 0];

        //return remaining bytes
        return inputBytes[(offset + 1)..];
    }

    public override string ToString(int tabSpace = 0)
    {
        return new string('\t', tabSpace)
            + $"TAG_Byte({((Name.Length == 0) ? "None" : "\'" + Name + "\'")}): {Value}";
    }
}
