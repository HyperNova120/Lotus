using LotusCore.Exceptions;
using LotusCore.Utils.NBTInternals.BaseClasses;

namespace LotusCore.Utils.NBTInternals.Tags;

public class TAG_Compound : TAG_Base, TAG_Collection
{
    public Dictionary<string, TAG_Base> _Contained_Tags = new();
    public bool _IsNetworkNBT = false;

    public TAG_Compound()
    {
        _Type_ID = 10;
    }

    public override int ProcessBytes(byte[] inputBytes)
    {
        //this.IsNetworkNBT = IsNetworkNBT;

        if (_IsInListTag || this._IsNetworkNBT)
        {
            int adder = _IsNetworkNBT ? 1 : 0;
            return DecodePayload(_IsNetworkNBT ? inputBytes[1..] : inputBytes) + adder;
        }

        //type id
        int offset = 0;
        _Type_ID = inputBytes[offset++];

        //name length + decode
        int nameLength = inputBytes[offset++];
        nameLength <<= 8;
        nameLength |= inputBytes[offset++];

        for (int i = 0; i < nameLength; i++)
        {
            _Name += (char)inputBytes[offset++];
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
                        _Contained_Tags[tmp_byte._Name] = tmp_byte;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_SHORT:
                        TAG_Short tmp_short = new TAG_Short();
                        offset += tmp_short.ProcessBytes(inputBytes[offset..]);
                        _Contained_Tags[tmp_short._Name] = tmp_short;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_INT:
                        TAG_Int tmp_int = new TAG_Int();
                        offset += tmp_int.ProcessBytes(inputBytes[offset..]);
                        _Contained_Tags[tmp_int._Name] = tmp_int;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_LONG:
                        TAG_Long tmp_long = new TAG_Long();
                        offset += tmp_long.ProcessBytes(inputBytes[offset..]);
                        _Contained_Tags[tmp_long._Name] = tmp_long;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_FLOAT:
                        TAG_Float tmp_float = new TAG_Float();
                        offset += tmp_float.ProcessBytes(inputBytes[offset..]);
                        _Contained_Tags[tmp_float._Name] = tmp_float;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_DOUBLE:
                        TAG_Double tmp_double = new TAG_Double();
                        offset += tmp_double.ProcessBytes(inputBytes[offset..]);
                        _Contained_Tags[tmp_double._Name] = tmp_double;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_BYTE_ARRAY:
                        TAG_Byte_Array tmp_byte_array = new TAG_Byte_Array();
                        offset += tmp_byte_array.ProcessBytes(inputBytes[offset..]);
                        _Contained_Tags[tmp_byte_array._Name] = tmp_byte_array;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_STRING:
                        TAG_String tmp_string = new TAG_String();
                        offset += tmp_string.ProcessBytes(inputBytes[offset..]);
                        _Contained_Tags[tmp_string._Name] = tmp_string;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_LIST:
                        TAG_List tmp_list = new TAG_List();
                        offset += tmp_list.ProcessBytes(inputBytes[offset..]);
                        _Contained_Tags[tmp_list._Name] = tmp_list;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_COMPOUND:
                        TAG_Compound tmp_compound = new TAG_Compound();
                        offset += tmp_compound.ProcessBytes(inputBytes[offset..]);
                        _Contained_Tags[tmp_compound._Name] = tmp_compound;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_INT_ARRAY:
                        TAG_Int_Array tmp_int_array = new TAG_Int_Array();
                        offset += tmp_int_array.ProcessBytes(inputBytes[offset..]);
                        _Contained_Tags[tmp_int_array._Name] = tmp_int_array;
                        break;
                    case (int)TAG_Base.TagTypeID.TAG_LONG_ARRAY:
                        TAG_Long_Array tmp_long_array = new TAG_Long_Array();
                        offset += tmp_long_array.ProcessBytes(inputBytes[offset..]);
                        _Contained_Tags[tmp_long_array._Name] = tmp_long_array;
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
            + $"TAG_Compound({((_Name == null || _Name.Length == 0) ? "None" : "\'" + _Name + "\'")}): {_Contained_Tags.Count} entries";

        returner += "\n" + new string('\t', tabSpace) + "{";

        foreach (TAG_Base cur in _Contained_Tags.Values)
        {
            returner += "\n" + cur.ToString(tabSpace + 1);
        }

        return returner + "\n" + new string('\t', tabSpace) + "}";
    }

    public override byte[] GetBytes()
    {
        List<byte> returner = [.. GetIDAndNamesBytes()];
        foreach (TAG_Base tagBase in _Contained_Tags.Values)
        {
            returner.AddRange(tagBase.GetBytes());
        }
        returner.Add(0x00);
        return returner.ToArray();
    }

    public TAG_Base? TryGetTag(string Tag_Name)
    {
        if (_Contained_Tags.ContainsKey(Tag_Name))
        {
            return _Contained_Tags[Tag_Name];
        }

        foreach (var tag in _Contained_Tags)
        {
            if (tag.Value._Type_ID != (int)NBT.NBT_Tags.TAG_Compound)
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
        _Contained_Tags[Tag._Name] = Tag;
    }

    public bool RemoveTag(string Tag_Name)
    {
        return _Contained_Tags.Remove(Tag_Name);
    }

    public override TAG_Base Clone()
    {
        TAG_Compound returner = new();
        returner._IsNetworkNBT = _IsNetworkNBT;
        returner._IsInListTag = _IsInListTag;
        returner._Name = _Name;
        foreach (var tmp in _Contained_Tags)
        {
            returner._Contained_Tags[tmp.Key] = tmp.Value.Clone();
        }
        return returner;
    }

    internal void Combine(TAG_Compound value, bool overwrite)
    {
        if (value._Contained_Tags.Count == 0)
        {
            return;
        }
        else if (_Contained_Tags.Count == 0)
        {
            foreach (var item in value._Contained_Tags)
            {
                _Contained_Tags[item.Key] = item.Value.Clone();
            }
            return;
        }

        foreach (var item in value._Contained_Tags)
        {
            if (!_Contained_Tags.ContainsKey(item.Key))
            {
                _Contained_Tags[item.Key] = item.Value.Clone();
                continue;
            }

            if (
                item.Value is TAG_Compound tmp_compound
                && _Contained_Tags[item.Key] is TAG_Compound
            )
            {
                ((TAG_Compound)_Contained_Tags[item.Key]).Combine(tmp_compound, overwrite);
            }
            else if (overwrite)
            {
                _Contained_Tags[item.Key] = item.Value.Clone();
            }
        }
    }
}
