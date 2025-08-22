using Core_Engine.Exceptions;
using Core_Engine.Utils.NBTInternals.BaseClasses;

namespace Core_Engine.Utils.NBTInternals.Tags;

public class TAG_List : TAG_Base, TAG_Collection
{
    public int _Contained_Tag_Type;

    bool _AcceptAnyType = false;

    int _length;

    public List<TAG_Base> _Contained_Tags = new();

    public TAG_List()
    {
        _length = 0;
        _Type_ID = 9;
    }

    public override TAG_Base Clone()
    {
        TAG_List ret = new();
        ret._IsInListTag = _IsInListTag;
        ret._Name = _Name;
        ret._AcceptAnyType = _AcceptAnyType;
        ret._Contained_Tag_Type = _Contained_Tag_Type;
        foreach (var tmp in _Contained_Tags)
        {
            ret._Contained_Tags.Add(tmp.Clone());
        }
        return ret;
    }

    public TAG_List(string Name)
    {
        _length = 0;
        _Type_ID = 9;
        this._Name = Name;
    }

    public override int ProcessBytes(byte[] inputBytes)
    {
        int offset = ProcessIDAndNameBytes(inputBytes);
        _Contained_Tag_Type = inputBytes[offset++];

        //length = BitConverter.ToInt32(inputBytes[1..5].Reverse().ToArray(), 0);
        _length = BitConverter.ToInt32(
            [
                inputBytes[offset + 3],
                inputBytes[offset + 2],
                inputBytes[offset + 1],
                inputBytes[offset + 0],
            ],
            0
        );
        offset += 4;

        if (_length <= 0)
        {
            //list type may be tag end; parsers accept any type instead
            _AcceptAnyType = true;
            _length = 0;
        }

        for (int i = 0; i < _length; i++)
        {
            int currentTagID = _AcceptAnyType ? inputBytes[offset] : _Contained_Tag_Type;
            //++offset;
            if (!_AcceptAnyType && currentTagID != _Contained_Tag_Type)
            {
                throw new IncorrectNBTTypeException(
                    $"List is marked to only contain tag type {_Contained_Tag_Type} however received tag type {currentTagID}"
                );
            }
            switch (currentTagID)
            {
                case (int)TAG_Base.TagTypeID.TAG_END:
                    TAG_End tmp_end = new TAG_End();
                    tmp_end._IsInListTag = true;
                    offset += tmp_end.ProcessBytes(inputBytes[offset..]);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_BYTE:
                    TAG_Byte tmp_byte = new TAG_Byte();
                    tmp_byte._IsInListTag = true;
                    offset += tmp_byte.ProcessBytes(inputBytes[offset..]);
                    _Contained_Tags.Add(tmp_byte);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_SHORT:
                    TAG_Short tmp_short = new TAG_Short();
                    tmp_short._IsInListTag = true;
                    offset += tmp_short.ProcessBytes(inputBytes[offset..]);
                    _Contained_Tags.Add(tmp_short);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_INT:
                    TAG_Int tmp_int = new TAG_Int();
                    tmp_int._IsInListTag = true;
                    offset += tmp_int.ProcessBytes(inputBytes[offset..]);
                    _Contained_Tags.Add(tmp_int);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_LONG:
                    TAG_Long tmp_long = new TAG_Long();
                    tmp_long._IsInListTag = true;
                    offset += tmp_long.ProcessBytes(inputBytes[offset..]);
                    _Contained_Tags.Add(tmp_long);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_FLOAT:
                    TAG_Float tmp_float = new TAG_Float();
                    tmp_float._IsInListTag = true;
                    offset += tmp_float.ProcessBytes(inputBytes[offset..]);
                    _Contained_Tags.Add(tmp_float);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_DOUBLE:
                    TAG_Double tmp_double = new TAG_Double();
                    tmp_double._IsInListTag = true;
                    offset += tmp_double.ProcessBytes(inputBytes[offset..]);
                    _Contained_Tags.Add(tmp_double);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_BYTE_ARRAY:
                    TAG_Byte_Array tmp_byte_array = new TAG_Byte_Array();
                    tmp_byte_array._IsInListTag = true;
                    offset += tmp_byte_array.ProcessBytes(inputBytes[offset..]);
                    _Contained_Tags.Add(tmp_byte_array);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_STRING:
                    TAG_String tmp_string = new TAG_String();
                    tmp_string._IsInListTag = true;
                    offset += tmp_string.ProcessBytes(inputBytes[offset..]);
                    _Contained_Tags.Add(tmp_string);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_LIST:
                    TAG_List tmp_list = new TAG_List();
                    tmp_list._IsInListTag = true;
                    offset += tmp_list.ProcessBytes(inputBytes[offset..]);
                    _Contained_Tags.Add(tmp_list);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_COMPOUND:
                    TAG_Compound tmp_compound = new TAG_Compound();
                    tmp_compound._IsInListTag = true;
                    offset += tmp_compound.ProcessBytes(inputBytes[offset..]);
                    _Contained_Tags.Add(tmp_compound);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_INT_ARRAY:
                    TAG_Int_Array tmp_int_array = new TAG_Int_Array();
                    tmp_int_array._IsInListTag = true;
                    offset += tmp_int_array.ProcessBytes(inputBytes[offset..]);
                    _Contained_Tags.Add(tmp_int_array);
                    break;
                case (int)TAG_Base.TagTypeID.TAG_LONG_ARRAY:
                    TAG_Long_Array tmp_long_array = new TAG_Long_Array();
                    tmp_long_array._IsInListTag = true;
                    offset += tmp_long_array.ProcessBytes(inputBytes[offset..]);
                    _Contained_Tags.Add(tmp_long_array);
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
            + $"TAG_List({((_Name.Length == 0) ? "None" : "\'" + _Name + "\'")}): {_Contained_Tags.Count} entries";

        returner += "\n" + new string('\t', tabSpace) + "{";

        foreach (TAG_Base cur in _Contained_Tags)
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
            (byte)_Contained_Tag_Type,
            .. BitConverter.GetBytes(_Contained_Tags.Count).Reverse(),
        ];

        foreach (TAG_Base tagBase in _Contained_Tags)
        {
            returner.AddRange(tagBase.GetBytes());
        }
        return returner.ToArray();
    }

    public TAG_Base? TryGetTag(string Tag_Name)
    {
        for (int i = 0; i < _Contained_Tags.Count; i++)
        {
            if (_Contained_Tags[i]._Name == Tag_Name)
            {
                return _Contained_Tags[i];
            }
        }
        return null;
    }

    public void WriteTag<T>(T Tag)
        where T : TAG_Base
    {
        for (int i = 0; i < _Contained_Tags.Count; i++)
        {
            if (_Contained_Tags[i]._Name == Tag._Name)
            {
                return;
            }
        }
        _Contained_Tags.Add(Tag);
    }

    public bool RemoveTag(string Tag_Name)
    {
        for (int i = 0; i < _Contained_Tags.Count; i++)
        {
            if (_Contained_Tags[i]._Name == Tag_Name)
            {
                _Contained_Tags.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    internal void Combine(TAG_List value, bool overwrite)
    {
        if (value._Contained_Tags.Count == 0)
        {
            return;
        }
        else if (_Contained_Tags.Count == 0)
        {
            foreach (var item in value._Contained_Tags)
            {
                _Contained_Tags.Add(item.Clone());
            }
            return;
        }

        foreach (var item in value._Contained_Tags)
        {
            if (!_Contained_Tags.Contains(item))
            {
                _Contained_Tags.Add(item.Clone());
                continue;
            }

            int index = _Contained_Tags.IndexOf(item);

            if (item is TAG_Compound tmp_compound && _Contained_Tags[index] is TAG_Compound)
            {
                ((TAG_Compound)_Contained_Tags[index]).Combine(tmp_compound, overwrite);
            }
            else if (item is TAG_List tmp_list && _Contained_Tags[index] is TAG_List)
            {
                ((TAG_List)_Contained_Tags[index]).Combine(tmp_list, overwrite);
            }
            else if (overwrite)
            {
                _Contained_Tags.Add(item.Clone());
            }
        }
    }
}
