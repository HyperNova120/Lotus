using Core_Engine.Exceptions;
using Core_Engine.Utils.NBTInternals.BaseClasses;

namespace Core_Engine.Utils.NBTInternals.Tags;

public class TAG_Compound : TAG_Base, TAG_Collection
{
    public Dictionary<string, TAG_Base> Contained_Tags = new();
    public bool IsNetworkNBT = false;

    public TAG_Compound()
    {
        Type_ID = 10;
    }

    public override int ProcessBytes(byte[] inputBytes)
    {
        //this.IsNetworkNBT = IsNetworkNBT;

        if (isInListTag || this.IsNetworkNBT)
        {
            int adder = IsNetworkNBT ? 1 : 0;
            return DecodePayload(IsNetworkNBT ? inputBytes[1..] : inputBytes) + adder;
        }

        //type id
        int offset = 0;
        Type_ID = inputBytes[offset++];

        //name length + decode
        int nameLength = inputBytes[offset++];
        nameLength <<= 8;
        nameLength |= inputBytes[offset++];

        for (int i = 0; i < nameLength; i++)
        {
            Name += (char)inputBytes[offset++];
        }
        //offset += nameLength;
        return DecodePayload(inputBytes[offset..]) + offset;
    }

    private int DecodePayload(byte[] inputBytes)
    {
        //payload decode
        bool shouldRun = true;
        int index = 0;
        int offset = 0;
        while (inputBytes.Length > offset && shouldRun)
        {
            int payloadType = inputBytes[offset];

            try
            {
                switch (payloadType)
                {
                    case (int)TAG_Base.TagTypeID.TAG_END:
                        TAG_End tmp_end = new TAG_End();
                        offset += tmp_end.ProcessBytes(inputBytes[offset..]);
                        shouldRun = false;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_BYTE:
                        TAG_Byte tmp_byte = new TAG_Byte();
                        offset += tmp_byte.ProcessBytes(inputBytes[offset..]);
                        Contained_Tags[tmp_byte.Name] = tmp_byte;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_SHORT:
                        TAG_Short tmp_short = new TAG_Short();
                        offset += tmp_short.ProcessBytes(inputBytes[offset..]);
                        Contained_Tags[tmp_short.Name] = tmp_short;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_INT:
                        TAG_Int tmp_int = new TAG_Int();
                        offset += tmp_int.ProcessBytes(inputBytes[offset..]);
                        Contained_Tags[tmp_int.Name] = tmp_int;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_LONG:
                        TAG_Long tmp_long = new TAG_Long();
                        offset += tmp_long.ProcessBytes(inputBytes[offset..]);
                        Contained_Tags[tmp_long.Name] = tmp_long;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_FLOAT:
                        TAG_Float tmp_float = new TAG_Float();
                        offset += tmp_float.ProcessBytes(inputBytes[offset..]);
                        Contained_Tags[tmp_float.Name] = tmp_float;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_DOUBLE:
                        TAG_Double tmp_double = new TAG_Double();
                        offset += tmp_double.ProcessBytes(inputBytes[offset..]);
                        Contained_Tags[tmp_double.Name] = tmp_double;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_BYTE_ARRAY:
                        TAG_Byte_Array tmp_byte_array = new TAG_Byte_Array();
                        offset += tmp_byte_array.ProcessBytes(inputBytes[offset..]);
                        Contained_Tags[tmp_byte_array.Name] = tmp_byte_array;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_STRING:
                        TAG_String tmp_string = new TAG_String();
                        offset += tmp_string.ProcessBytes(inputBytes[offset..]);
                        Contained_Tags[tmp_string.Name] = tmp_string;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_LIST:
                        TAG_List tmp_list = new TAG_List();
                        offset += tmp_list.ProcessBytes(inputBytes[offset..]);
                        Contained_Tags[tmp_list.Name] = tmp_list;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_COMPOUND:
                        TAG_Compound tmp_compound = new TAG_Compound();
                        offset += tmp_compound.ProcessBytes(inputBytes[offset..]);
                        Contained_Tags[tmp_compound.Name] = tmp_compound;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_INT_ARRAY:
                        TAG_Int_Array tmp_int_array = new TAG_Int_Array();
                        offset += tmp_int_array.ProcessBytes(inputBytes[offset..]);
                        Contained_Tags[tmp_int_array.Name] = tmp_int_array;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_LONG_ARRAY:
                        TAG_Long_Array tmp_long_array = new TAG_Long_Array();
                        offset += tmp_long_array.ProcessBytes(inputBytes[offset..]);
                        Contained_Tags[tmp_long_array.Name] = tmp_long_array;
                        break;
                    default:
                        throw new IncorrectNBTTypeException(
                            $"{index}: something went wrong Compound, incorrect type id: {payloadType}"
                        );
                }
            }
            catch (Exception e)
            {
                Logging.LogError(
                    $"offset:{offset} size:{inputBytes.Length} payload_type:{payloadType} index:{index}\nException:{e.ToString()}"
                );
                throw;
            }
            ++index;
        }

        return offset;
    }

    public override string ToString(int tabSpace = 0)
    {
        string returner =
            new string('\t', tabSpace)
            + $"TAG_Compound({((Name == null || Name.Length == 0) ? "None" : "\'" + Name + "\'")}): {Contained_Tags.Count} entries";

        returner += "\n" + new string('\t', tabSpace) + "{";

        foreach (TAG_Base cur in Contained_Tags.Values)
        {
            returner += "\n" + cur.ToString(tabSpace + 1);
        }

        return returner + "\n" + new string('\t', tabSpace) + "}";
    }

    public override byte[] GetBytes()
    {
        List<byte> returner = [.. GetIDAndNamesBytes()];
        foreach (TAG_Base tagBase in Contained_Tags.Values)
        {
            returner.AddRange(tagBase.GetBytes());
        }
        returner.Add(0);
        return returner.ToArray();
    }

    public TAG_Base? TryGetTag(string Tag_Name)
    {
        if (Contained_Tags.ContainsKey(Tag_Name))
        {
            return Contained_Tags[Tag_Name];
        }

        foreach (var tag in Contained_Tags)
        {
            if (tag.Value.Type_ID != (int)NBT.NBT_Tags.TAG_Compound)
            {
                continue;
            }
            TAG_Base? tmp = ((TAG_Compound)tag.Value).TryGetTag(Tag_Name);
            if (tmp == null)
            {
                continue;
            }
            return tmp;
        }

        return null;
    }

    public void WriteTag<T>(T Tag)
        where T : TAG_Base
    {
        Contained_Tags[Tag.Name] = Tag;
    }

    public bool RemoveTag(string Tag_Name)
    {
        return Contained_Tags.Remove(Tag_Name);
    }
}
