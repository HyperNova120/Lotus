using System.Text;
using Core_Engine.Utils.NBT.BaseClasses;

namespace Core_Engine.Utils.NBT.Tags;

public class TAG_String : TAG_Base
{
    public string Value = "";

    public TAG_String()
    {
        Type_ID = 8;
    }

    public override byte[] ProcessBytes(byte[] inputBytes)
    {
        inputBytes = ProcessIDAndNameBytes(inputBytes);

        ushort length = inputBytes[0];
        length <<= 8;
        length |= inputBytes[1];

        /* for (int i = 0; i < length; i++)
        {
            Value += (char)inputBytes[2 + i];
        } */
        Value = Encoding.UTF8.GetString(inputBytes[2..(2 + length)]);

        return inputBytes[(2 + length)..];
    }

    public override string ToString(int tabSpace = 0)
    {
        //string returnValue = Encoding.UTF8.GetString(Encoding.Default.GetBytes(Value));
        return new string('\t', tabSpace)
            + $"TAG_String({((Name.Length == 0) ? "None" : "\'" + Name + "\'")}): \'{Value}\'";
    }
}
