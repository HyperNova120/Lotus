using Core_Engine.Exceptions;
using Core_Engine.Utils.NBTInternals.BaseClasses;

namespace Core_Engine.Utils.NBTInternals.Tags;

public class TAG_List : TAG_Base, TAG_Collection
{
    public int Contained_Tag_Type;

    bool AcceptAnyType = false;

    int length;

    public List<TAG_Base> Contained_Tags = new();

    public TAG_List()
    {
        length = 0;
        Type_ID = 9;
    }

    public TAG_List(string Name)
    {
        length = 0;
        Type_ID = 9;
        this.Name = Name;
    }

    public override int ProcessBytes(byte[] inputBytes)
    {
        int offset = ProcessIDAndNameBytes(inputBytes);
        Contained_Tag_Type = inputBytes[offset++];

        //length = BitConverter.ToInt32(inputBytes[1..5].Reverse().ToArray(), 0);
        length = BitConverter.ToInt32(
            [
                inputBytes[offset + 3],
                inputBytes[offset + 2],
                inputBytes[offset + 1],
                inputBytes[offset + 0],
            ],
            0
        );
        offset += 4;

        if (length <= 0)
        {
            //list type may be tag end; parsers accept any type instead
            AcceptAnyType = true;
            length = 0;
        }

        for (int i = 0; i < length; i++)
        {
            int currentTagID = AcceptAnyType ? inputBytes[offset] : Contained_Tag_Type;
            //++offset;
            if (!AcceptAnyType && currentTagID != Contained_Tag_Type)
            {
                throw new IncorrectNBTTypeException(
                    $"List is marked to only contain tag type {Contained_Tag_Type} however received tag type {currentTagID}"
                );
            }
            switch (currentTagID)
            {
                case (int)TAG_Base.TagTypeID.TAG_END:
                    TAG_End tmp_end = new TAG_End();
                    tmp_end.isInListTag = true;
                    offset += tmp_end.ProcessBytes(inputBytes[offset..]);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_BYTE:
                    TAG_Byte tmp_byte = new TAG_Byte();
                    tmp_byte.isInListTag = true;
                    offset += tmp_byte.ProcessBytes(inputBytes[offset..]);
                    Contained_Tags.Add(tmp_byte);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_SHORT:
                    TAG_Short tmp_short = new TAG_Short();
                    tmp_short.isInListTag = true;
                    offset += tmp_short.ProcessBytes(inputBytes[offset..]);
                    Contained_Tags.Add(tmp_short);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_INT:
                    TAG_Int tmp_int = new TAG_Int();
                    tmp_int.isInListTag = true;
                    offset += tmp_int.ProcessBytes(inputBytes[offset..]);
                    Contained_Tags.Add(tmp_int);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_LONG:
                    TAG_Long tmp_long = new TAG_Long();
                    tmp_long.isInListTag = true;
                    offset += tmp_long.ProcessBytes(inputBytes[offset..]);
                    Contained_Tags.Add(tmp_long);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_FLOAT:
                    TAG_Float tmp_float = new TAG_Float();
                    tmp_float.isInListTag = true;
                    offset += tmp_float.ProcessBytes(inputBytes[offset..]);
                    Contained_Tags.Add(tmp_float);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_DOUBLE:
                    TAG_Double tmp_double = new TAG_Double();
                    tmp_double.isInListTag = true;
                    offset += tmp_double.ProcessBytes(inputBytes[offset..]);
                    Contained_Tags.Add(tmp_double);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_BYTE_ARRAY:
                    TAG_Byte_Array tmp_byte_array = new TAG_Byte_Array();
                    tmp_byte_array.isInListTag = true;
                    offset += tmp_byte_array.ProcessBytes(inputBytes[offset..]);
                    Contained_Tags.Add(tmp_byte_array);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_STRING:
                    TAG_String tmp_string = new TAG_String();
                    tmp_string.isInListTag = true;
                    offset += tmp_string.ProcessBytes(inputBytes[offset..]);
                    Contained_Tags.Add(tmp_string);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_LIST:
                    TAG_List tmp_list = new TAG_List();
                    tmp_list.isInListTag = true;
                    offset += tmp_list.ProcessBytes(inputBytes[offset..]);
                    Contained_Tags.Add(tmp_list);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_COMPOUND:
                    TAG_Compound tmp_compound = new TAG_Compound();
                    tmp_compound.isInListTag = true;
                    offset += tmp_compound.ProcessBytes(inputBytes[offset..]);
                    Contained_Tags.Add(tmp_compound);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_INT_ARRAY:
                    TAG_Int_Array tmp_int_array = new TAG_Int_Array();
                    tmp_int_array.isInListTag = true;
                    offset += tmp_int_array.ProcessBytes(inputBytes[offset..]);
                    Contained_Tags.Add(tmp_int_array);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_LONG_ARRAY:
                    TAG_Long_Array tmp_long_array = new TAG_Long_Array();
                    tmp_long_array.isInListTag = true;
                    offset += tmp_long_array.ProcessBytes(inputBytes[offset..]);
                    Contained_Tags.Add(tmp_long_array);
                    break;
                default:
                    throw new IncorrectNBTTypeException(
                        $"{i}: something went wrong List, incorrect type id: {currentTagID}"
                    );
            }
        }
        return offset;
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

    public override byte[] GetBytes()
    {
        List<byte> returner =
        [
            .. GetIDAndNamesBytes(),
            (byte)Contained_Tag_Type,
            .. BitConverter.GetBytes(Contained_Tags.Count).Reverse(),
        ];

        foreach (TAG_Base tagBase in Contained_Tags)
        {
            returner.AddRange(tagBase.GetBytes());
        }
        return returner.ToArray();
    }

    public TAG_Base? TryGetTag(string Tag_Name)
    {
        for (int i = 0; i < Contained_Tags.Count; i++)
        {
            if (Contained_Tags[i].Name == Tag_Name)
            {
                return Contained_Tags[i];
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
                return;
            }
        }
        Contained_Tags.Add(Tag);
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
}
