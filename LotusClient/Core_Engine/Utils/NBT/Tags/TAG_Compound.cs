using Core_Engine.Utils.NBT.BaseClasses;
using SharpNBT;

namespace Core_Engine.Utils.NBT.Tags;

public class TAG_Compound : TAG_Base, TAG_Collection
{
    private List<TAG_Base> Contained_Tags = new();
    public bool IsNetworkNBT = false;

    public TAG_Compound()
    {
        Type_ID = 10;
    }

    public override byte[] ProcessBytes(byte[] inputBytes)
    {
        return ProcessBytes(inputBytes, false);
    }

    public byte[] ProcessBytes(byte[] inputBytes, bool IsNetworkNBT)
    {
        this.IsNetworkNBT = IsNetworkNBT;

        if (isInListTag)
        {
            return DecodePayload(inputBytes, IsNetworkNBT);
        }

        //type id
        Type_ID = inputBytes[0];
        inputBytes = inputBytes[1..];

        //name length + decode
        int nameLength = inputBytes[0];
        nameLength <<= 8;
        nameLength |= inputBytes[1];
        inputBytes = inputBytes[2..];

        for (int i = 0; i < nameLength; i++)
        {
            Name += (char)inputBytes[i];
        }
        inputBytes = inputBytes[nameLength..];
        return DecodePayload(inputBytes, IsNetworkNBT);
    }

    private byte[] DecodePayload(byte[] inputBytes, bool IsNetworkNBT)
    {
        //payload decode
        bool shouldRun = true;
        while (inputBytes.Length > 0 && shouldRun)
        {
            int payloadType = inputBytes[0];

            switch (payloadType)
            {
                case 0:
                    TAG_End tmp_end = new TAG_End();
                    inputBytes = tmp_end.ProcessBytes(inputBytes);
                    shouldRun = false;
                    break;
                case 1:
                    TAG_Byte tmp_byte = new TAG_Byte();
                    inputBytes = tmp_byte.ProcessBytes(inputBytes);
                    Contained_Tags.Add(tmp_byte);
                    break;
                case 2:
                    TAG_Short tmp_short = new TAG_Short();
                    inputBytes = tmp_short.ProcessBytes(inputBytes);
                    Contained_Tags.Add(tmp_short);
                    break;
                case 3:
                    TAG_Int tmp_int = new TAG_Int();
                    inputBytes = tmp_int.ProcessBytes(inputBytes);
                    Contained_Tags.Add(tmp_int);
                    break;
                case 4:
                    TAG_Long tmp_long = new TAG_Long();
                    inputBytes = tmp_long.ProcessBytes(inputBytes);
                    Contained_Tags.Add(tmp_long);
                    break;
                case 5:
                    TAG_Float tmp_float = new TAG_Float();
                    inputBytes = tmp_float.ProcessBytes(inputBytes);
                    Contained_Tags.Add(tmp_float);
                    break;
                case 6:
                    TAG_Double tmp_double = new TAG_Double();
                    inputBytes = tmp_double.ProcessBytes(inputBytes);
                    Contained_Tags.Add(tmp_double);
                    break;
                case 7:
                    TAG_Byte_Array tmp_byte_array = new TAG_Byte_Array();
                    inputBytes = tmp_byte_array.ProcessBytes(inputBytes);
                    Contained_Tags.Add(tmp_byte_array);
                    break;
                case 8:
                    TAG_String tmp_string = new TAG_String();
                    inputBytes = tmp_string.ProcessBytes(inputBytes);
                    Contained_Tags.Add(tmp_string);
                    break;
                case 9:
                    TAG_List tmp_list = new TAG_List();
                    inputBytes = tmp_list.ProcessBytes(inputBytes, IsNetworkNBT);
                    Contained_Tags.Add(tmp_list);
                    break;
                case 10:
                    TAG_Compound tmp_compound = new TAG_Compound();
                    inputBytes = tmp_compound.ProcessBytes(inputBytes, IsNetworkNBT);
                    Contained_Tags.Add(tmp_compound);
                    break;
                case 11:
                    TAG_Int_Array tmp_int_array = new TAG_Int_Array();
                    inputBytes = tmp_int_array.ProcessBytes(inputBytes);
                    Contained_Tags.Add(tmp_int_array);
                    break;
                case 12:
                    TAG_Long_Array tmp_long_array = new TAG_Long_Array();
                    inputBytes = tmp_long_array.ProcessBytes(inputBytes);
                    Contained_Tags.Add(tmp_long_array);
                    break;
                default:
                    Logging.LogError($"something went wrong, incorrect type id: {payloadType}");
                    break;
            }
        }

        return inputBytes;
    }

    public bool RemoveTag(string Tag_Name)
    {
        for (int i = 0; i < Contained_Tags.Count; i++)
        {
            if (Contained_Tags[i].Name == Tag_Name)
            {
                Contained_Tags.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public T? TryGetTag<T>(string Tag_Name)
        where T : TAG_Base
    {
        foreach (TAG_Base Tag in Contained_Tags)
        {
            if (Tag.Name == Tag_Name && Tag is T)
            {
                return (T)Tag;
            }
        }
        return null;
    }

    public void WriteTag<T>(T Tag)
        where T : TAG_Base
    {
        for (int i = 0; i < Contained_Tags.Count; i++)
        {
            if (Contained_Tags[i].Name == Tag.Name)
            {
                Contained_Tags[i] = Tag;
                return;
            }
        }
        Contained_Tags.Add(Tag);
    }

    public override string ToString(int tabSpace = 0)
    {
        string returner =
            new string('\t', tabSpace)
            + $"TAG_Compount({((Name.Length == 0) ? "None" : "\'" + Name + "\'")}): {Contained_Tags.Count} entries";

        returner += "\n" + new string('\t', tabSpace) + "{";

        foreach (TAG_Base cur in Contained_Tags)
        {
            returner += "\n" + cur.ToString(tabSpace + 1);
        }

        return returner + "\n" + new string('\t', tabSpace) + "}";
    }
}
