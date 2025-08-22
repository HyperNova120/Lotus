using System.Diagnostics;
using Core_Engine.Utils.NBTInternals.BaseClasses;
using Core_Engine.Utils.NBTInternals.Tags;

namespace Core_Engine.Utils;

public class NBT
{
    public enum NBT_Tags
    {
        TAG_End,
        TAG_Byte,
        TAG_Short,
        TAG_Int,
        TAG_Long,
        TAG_Float,
        TAG_Double,
        TAG_Byte_Array,
        TAG_String,
        TAG_List,
        TAG_Compound,
        TAG_Int_Array,
        TAG_Long_Array,
    }

    private TAG_Compound? _Base_Tag = null;

    public NBT(bool IsNetworkNBT = false)
    {
        _Base_Tag = new() { _Name = IsNetworkNBT ? null : "", _IsNetworkNBT = IsNetworkNBT };
    }

    public NBT(string Compound_Name)
    {
        _Base_Tag = new() { _Name = Compound_Name };
    }

    public bool RemoveTag(string Tag_Name)
    {
        if (_Base_Tag == null)
        {
            return false;
        }
        return _Base_Tag.RemoveTag(Tag_Name);
    }

    public int ReadFromBytes(byte[] bytes, bool networkBytes = false)
    {
        _Base_Tag = new() { _IsNetworkNBT = networkBytes };
        int numBytesRead = _Base_Tag.ProcessBytes(bytes);

        /* if (networkBytes)
        {
            base_Tag = (TAG_Compound)base_Tag.Contained_Tags.First().Value;
            base_Tag.IsNetworkNBT = networkBytes;
        } */

        return numBytesRead;
    }

    public string GetNBTAsString(int tab_space = 0)
    {
        return (_Base_Tag != null) ? _Base_Tag.ToString(tab_space) : "";
    }

    public byte[] GetBytes()
    {
        return (_Base_Tag != null) ? _Base_Tag.GetBytes() : [];
    }

    public T? TryGetTag<T>(string Tag_Name)
        where T : TAG_Base
    {
        if (_Base_Tag == null)
        {
            return null;
        }

        if (_Base_Tag._Name == Tag_Name)
        {
            return _Base_Tag as T;
        }

        foreach (var tag in _Base_Tag!._Contained_Tags)
        {
            if (tag.Key == Tag_Name)
            {
                return tag.Value as T;
            }
            else if (tag.Value._Type_ID == (int)NBT_Tags.TAG_Compound)
            {
                var compound = (TAG_Compound)tag.Value;
                var nestedTag = compound.TryGetTag(Tag_Name);
                if (nestedTag != null)
                {
                    return nestedTag as T;
                }
            }
        }
        return null;
    }

    public NBT WriteTag(string name, byte data)
    {
        _Base_Tag!.WriteTag<TAG_Byte>(new() { _Name = name, Value = (sbyte)data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<byte> data)
    {
        TAG_List tmp = new(name) { _Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_BYTE };
        foreach (byte b in data)
        {
            tmp._Contained_Tags.Add(new TAG_Byte() { Value = (sbyte)b, _IsInListTag = true });
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, short data)
    {
        _Base_Tag!.WriteTag<TAG_Short>(new() { _Name = name, Value = data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<short> data)
    {
        TAG_List tmp = new(name) { _Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_SHORT };
        foreach (short b in data)
        {
            tmp._Contained_Tags.Add(new TAG_Short() { Value = b, _IsInListTag = true });
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, int data)
    {
        _Base_Tag!.WriteTag<TAG_Int>(new() { _Name = name, Value = data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<int> data)
    {
        TAG_List tmp = new(name) { _Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_INT };
        foreach (int b in data)
        {
            tmp._Contained_Tags.Add(new TAG_Int() { Value = b, _IsInListTag = true });
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, long data)
    {
        _Base_Tag!.WriteTag<TAG_Long>(new() { _Name = name, Value = data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<long> data)
    {
        TAG_List tmp = new(name) { _Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_LONG };
        foreach (long b in data)
        {
            tmp._Contained_Tags.Add(new TAG_Long() { Value = b, _IsInListTag = true });
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, float data)
    {
        _Base_Tag!.WriteTag<TAG_Float>(new() { _Name = name, Value = data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<float> data)
    {
        TAG_List tmp = new(name) { _Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_FLOAT };
        foreach (float b in data)
        {
            tmp._Contained_Tags.Add(new TAG_Float() { Value = b, _IsInListTag = true });
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, double data)
    {
        _Base_Tag!.WriteTag<TAG_Double>(new() { _Name = name, Value = data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<double> data)
    {
        TAG_List tmp = new(name) { _Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_DOUBLE };
        foreach (double b in data)
        {
            tmp._Contained_Tags.Add(new TAG_Double() { Value = b, _IsInListTag = true });
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, byte[] data)
    {
        _Base_Tag!.WriteTag<TAG_Byte_Array>(new() { _Name = name, Values = (sbyte[])(Array)data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<byte[]> data)
    {
        TAG_List tmp = new(name) { _Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_BYTE_ARRAY };
        foreach (byte[] b in data)
        {
            tmp._Contained_Tags.Add(
                new TAG_Byte_Array() { Values = (sbyte[])(Array)b, _IsInListTag = true }
            );
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, string data)
    {
        _Base_Tag!.WriteTag<TAG_String>(new() { _Name = name, Value = data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<string> data)
    {
        TAG_List tmp = new(name) { _Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_STRING };
        foreach (string b in data)
        {
            tmp._Contained_Tags.Add(new TAG_String() { Value = b, _IsInListTag = true });
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, int[] data)
    {
        _Base_Tag!.WriteTag<TAG_Int_Array>(new() { _Name = name, Values = data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<int[]> data)
    {
        TAG_List tmp = new(name) { _Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_INT_ARRAY };
        foreach (int[] b in data)
        {
            tmp._Contained_Tags.Add(new TAG_Int_Array() { Values = b, _IsInListTag = true });
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, long[] data)
    {
        _Base_Tag!.WriteTag<TAG_Long_Array>(new() { _Name = name, Values = data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<long[]> data)
    {
        TAG_List tmp = new(name) { _Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_LONG_ARRAY };
        foreach (long[] b in data)
        {
            tmp._Contained_Tags.Add(new TAG_Long_Array() { Values = b, _IsInListTag = true });
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, NBT data)
    {
        data._Base_Tag!._Name = name;
        _Base_Tag!.WriteTag(data._Base_Tag!);
        return this;
    }

    public NBT WriteTag(NBT data)
    {
        _Base_Tag!.WriteTag(data._Base_Tag!);
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<NBT> data)
    {
        TAG_List tmp = new(name) { _Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_COMPOUND };
        foreach (NBT b in data)
        {
            b._Base_Tag!._IsInListTag = true;
            tmp._Contained_Tags.Add(b._Base_Tag!);
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    /// <summary>
    /// combines two NBTs, non conflicting values are added.
    /// if there are conflicting values and overwrite is true, values from data
    /// take priority, else conflicting values from data are ignored
    /// </summary>
    /// <param name="data">NBT to combine to this one</param>
    /// <param name="overwrite">should data value take priority if conflicting values </param>
    internal NBT Combine(NBT data, bool overwrite = false)
    {
        if (data._Base_Tag == null)
        {
            return this;
        }
        else if (_Base_Tag == null)
        {
            _Base_Tag = (TAG_Compound)data._Base_Tag!.Clone();
            return this;
        }

        foreach (var value in data._Base_Tag._Contained_Tags)
        {
            if (!_Base_Tag._Contained_Tags.ContainsKey(value.Key))
            {
                _Base_Tag._Contained_Tags[value.Key] = value.Value.Clone();
                continue;
            }

            if (
                value.Value is TAG_Compound tmp_compound
                && _Base_Tag._Contained_Tags[value.Key] is TAG_Compound
            )
            {
                ((TAG_Compound)_Base_Tag._Contained_Tags[value.Key]).Combine(
                    tmp_compound,
                    overwrite
                );
            }
            else if (
                value.Value is TAG_List tmp_list
                && _Base_Tag._Contained_Tags[value.Key] is TAG_List
            )
            {
                ((TAG_List)_Base_Tag._Contained_Tags[value.Key]).Combine(tmp_list, overwrite);
            }
            else if (overwrite)
            {
                _Base_Tag._Contained_Tags[value.Key] = value.Value.Clone();
            }
        }
        return this;
    }

    internal NBT Clone()
    {
        NBT ret = new();
        ret._Base_Tag = (TAG_Compound?)_Base_Tag.Clone();
        return ret;
    }
}
