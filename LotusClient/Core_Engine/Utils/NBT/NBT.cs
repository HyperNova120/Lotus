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
        _Base_Tag = new() { Name = IsNetworkNBT ? null : "", IsNetworkNBT = IsNetworkNBT };
    }

    public NBT(string Compound_Name)
    {
        _Base_Tag = new() { Name = Compound_Name };
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
        _Base_Tag = new() { IsNetworkNBT = networkBytes };
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

        if (_Base_Tag.Name == Tag_Name)
        {
            return _Base_Tag as T;
        }

        foreach (var tag in _Base_Tag!.Contained_Tags)
        {
            if (tag.Key == Tag_Name)
            {
                return tag.Value as T;
            }
            else if (tag.Value.Type_ID == (int)NBT_Tags.TAG_Compound)
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
        _Base_Tag!.WriteTag<TAG_Byte>(new() { Name = name, Value = (sbyte)data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<byte> data)
    {
        TAG_List tmp = new(name) { Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_BYTE };
        foreach (byte b in data)
        {
            tmp.Contained_Tags.Add(new TAG_Byte() { Value = (sbyte)b, isInListTag = true });
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, short data)
    {
        _Base_Tag!.WriteTag<TAG_Short>(new() { Name = name, Value = data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<short> data)
    {
        TAG_List tmp = new(name) { Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_SHORT };
        foreach (short b in data)
        {
            tmp.Contained_Tags.Add(new TAG_Short() { Value = b, isInListTag = true });
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, int data)
    {
        _Base_Tag!.WriteTag<TAG_Int>(new() { Name = name, Value = data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<int> data)
    {
        TAG_List tmp = new(name) { Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_INT };
        foreach (int b in data)
        {
            tmp.Contained_Tags.Add(new TAG_Int() { Value = b, isInListTag = true });
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, long data)
    {
        _Base_Tag!.WriteTag<TAG_Long>(new() { Name = name, Value = data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<long> data)
    {
        TAG_List tmp = new(name) { Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_LONG };
        foreach (long b in data)
        {
            tmp.Contained_Tags.Add(new TAG_Long() { Value = b, isInListTag = true });
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, float data)
    {
        _Base_Tag!.WriteTag<TAG_Float>(new() { Name = name, Value = data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<float> data)
    {
        TAG_List tmp = new(name) { Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_FLOAT };
        foreach (float b in data)
        {
            tmp.Contained_Tags.Add(new TAG_Float() { Value = b, isInListTag = true });
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, double data)
    {
        _Base_Tag!.WriteTag<TAG_Double>(new() { Name = name, Value = data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<double> data)
    {
        TAG_List tmp = new(name) { Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_DOUBLE };
        foreach (double b in data)
        {
            tmp.Contained_Tags.Add(new TAG_Double() { Value = b, isInListTag = true });
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, byte[] data)
    {
        _Base_Tag!.WriteTag<TAG_Byte_Array>(new() { Name = name, Values = (sbyte[])(Array)data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<byte[]> data)
    {
        TAG_List tmp = new(name) { Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_BYTE_ARRAY };
        foreach (byte[] b in data)
        {
            tmp.Contained_Tags.Add(
                new TAG_Byte_Array() { Values = (sbyte[])(Array)b, isInListTag = true }
            );
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, string data)
    {
        _Base_Tag!.WriteTag<TAG_String>(new() { Name = name, Value = data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<string> data)
    {
        TAG_List tmp = new(name) { Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_STRING };
        foreach (string b in data)
        {
            tmp.Contained_Tags.Add(new TAG_String() { Value = b, isInListTag = true });
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, int[] data)
    {
        _Base_Tag!.WriteTag<TAG_Int_Array>(new() { Name = name, Values = data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<int[]> data)
    {
        TAG_List tmp = new(name) { Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_INT_ARRAY };
        foreach (int[] b in data)
        {
            tmp.Contained_Tags.Add(new TAG_Int_Array() { Values = b, isInListTag = true });
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, long[] data)
    {
        _Base_Tag!.WriteTag<TAG_Long_Array>(new() { Name = name, Values = data });
        return this;
    }

    public NBT WriteListTag(string name, IEnumerable<long[]> data)
    {
        TAG_List tmp = new(name) { Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_LONG_ARRAY };
        foreach (long[] b in data)
        {
            tmp.Contained_Tags.Add(new TAG_Long_Array() { Values = b, isInListTag = true });
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }

    public NBT WriteTag(string name, NBT data)
    {
        data._Base_Tag!.Name = name;
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
        TAG_List tmp = new(name) { Contained_Tag_Type = (int)TAG_Base.TagTypeID.TAG_COMPOUND };
        foreach (NBT b in data)
        {
            b._Base_Tag!.isInListTag = true;
            tmp.Contained_Tags.Add(b._Base_Tag!);
        }
        _Base_Tag!.WriteTag<TAG_List>(tmp);
        return this;
    }
}
