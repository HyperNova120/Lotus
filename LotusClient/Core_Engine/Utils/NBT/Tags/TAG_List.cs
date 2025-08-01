using Core_Engine.Exceptions;
using Core_Engine.Utils.NBT.BaseClasses;

namespace Core_Engine.Utils.NBT.Tags;

public class TAG_List : TAG_Base, TAG_Collection
{
    int Contained_Tag_Type;

    bool AcceptAnyType = false;

    int length;

    List<TAG_Base> Contained_Tags = new();

    public TAG_List()
    {
        length = 0;
        Type_ID = 9;
    }

    public override byte[] ProcessBytes(byte[] inputBytes)
    {
        int offset = ProcessIDAndNameBytes(inputBytes);
        Contained_Tag_Type = inputBytes[offset + 0];

        //length = BitConverter.ToInt32(inputBytes[1..5].Reverse().ToArray(), 0);
        length = BitConverter.ToInt32(
            [
                inputBytes[offset + 4],
                inputBytes[offset + 3],
                inputBytes[offset + 2],
                inputBytes[offset + 1],
            ],
            0
        );

        if (length <= 0)
        {
            //list type may be tag end; parsers accept any type instead
            AcceptAnyType = true;
            length = 1;
        }

        inputBytes = inputBytes[(offset + 5)..];
        for (int i = 0; i < length; i++)
        {
            int currentTagID = AcceptAnyType ? inputBytes[offset] : Contained_Tag_Type;
            if (!AcceptAnyType && currentTagID != Contained_Tag_Type)
            {
                throw new IncorrectNBTTypeException(
                    $"List is marked to only contain tag type {Contained_Tag_Type} however received tag type {currentTagID}"
                );
            }
            switch (currentTagID)
            {
                case 0:
                    TAG_End tmp_end = new TAG_End();
                    tmp_end.isInListTag = true;
                    inputBytes = tmp_end.ProcessBytes(inputBytes);
                    offset = 0;
                    break;
                case 1:
                    TAG_Byte tmp_byte = new TAG_Byte();
                    tmp_byte.isInListTag = true;
                    inputBytes = tmp_byte.ProcessBytes(inputBytes);
                    offset = 0;
                    Contained_Tags.Add(tmp_byte);
                    break;
                case 2:
                    TAG_Short tmp_short = new TAG_Short();
                    tmp_short.isInListTag = true;
                    inputBytes = tmp_short.ProcessBytes(inputBytes);
                    offset = 0;
                    Contained_Tags.Add(tmp_short);
                    break;
                case 3:
                    TAG_Int tmp_int = new TAG_Int();
                    tmp_int.isInListTag = true;
                    inputBytes = tmp_int.ProcessBytes(inputBytes);
                    offset = 0;
                    Contained_Tags.Add(tmp_int);
                    break;
                case 4:
                    TAG_Long tmp_long = new TAG_Long();
                    tmp_long.isInListTag = true;
                    inputBytes = tmp_long.ProcessBytes(inputBytes);
                    offset = 0;
                    Contained_Tags.Add(tmp_long);
                    break;
                case 5:
                    TAG_Float tmp_float = new TAG_Float();
                    tmp_float.isInListTag = true;
                    inputBytes = tmp_float.ProcessBytes(inputBytes);
                    offset = 0;
                    Contained_Tags.Add(tmp_float);
                    break;
                case 6:
                    TAG_Double tmp_double = new TAG_Double();
                    tmp_double.isInListTag = true;
                    inputBytes = tmp_double.ProcessBytes(inputBytes);
                    offset = 0;
                    Contained_Tags.Add(tmp_double);
                    break;
                case 7:
                    TAG_Byte_Array tmp_byte_array = new TAG_Byte_Array();
                    tmp_byte_array.isInListTag = true;
                    inputBytes = tmp_byte_array.ProcessBytes(inputBytes);
                    offset = 0;
                    Contained_Tags.Add(tmp_byte_array);
                    break;
                case 8:
                    TAG_String tmp_string = new TAG_String();
                    tmp_string.isInListTag = true;
                    inputBytes = tmp_string.ProcessBytes(inputBytes);
                    offset = 0;
                    Contained_Tags.Add(tmp_string);
                    break;
                case 9:
                    TAG_List tmp_list = new TAG_List();
                    tmp_list.isInListTag = true;
                    inputBytes = tmp_list.ProcessBytes(inputBytes);
                    offset = 0;
                    Contained_Tags.Add(tmp_list);
                    break;
                case 10:
                    TAG_Compound tmp_compound = new TAG_Compound();
                    tmp_compound.isInListTag = true;
                    inputBytes = tmp_compound.ProcessBytes(inputBytes);
                    offset = 0;
                    Contained_Tags.Add(tmp_compound);
                    break;
                case 11:
                    TAG_Int_Array tmp_int_array = new TAG_Int_Array();
                    tmp_int_array.isInListTag = true;
                    inputBytes = tmp_int_array.ProcessBytes(inputBytes);
                    offset = 0;
                    Contained_Tags.Add(tmp_int_array);
                    break;
                case 12:
                    TAG_Long_Array tmp_long_array = new TAG_Long_Array();
                    tmp_long_array.isInListTag = true;
                    inputBytes = tmp_long_array.ProcessBytes(inputBytes);
                    offset = 0;
                    Contained_Tags.Add(tmp_long_array);
                    break;
                default:
                    throw new IncorrectNBTTypeException(
                        $"{i}: something went wrong List, incorrect type id: {currentTagID}"
                    );
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
        foreach (TAG_Base tag in Contained_Tags)
        {
            if (tag.Name == Tag_Name && tag is T)
            {
                return (T)tag;
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
                //replace
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
            + $"TAG_List({((Name.Length == 0) ? "None" : "\'" + Name + "\'")}): {Contained_Tags.Count} entries";

        returner += "\n" + new string('\t', tabSpace) + "{";

        foreach (TAG_Base cur in Contained_Tags)
        {
            returner += "\n" + cur.ToString(tabSpace + 1);
        }

        return returner + "\n" + new string('\t', tabSpace) + "}";
    }
}
