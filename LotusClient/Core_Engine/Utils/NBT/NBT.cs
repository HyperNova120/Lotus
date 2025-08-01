using Core_Engine.Utils.NBT.BaseClasses;
using Core_Engine.Utils.NBT.Tags;

namespace Core_Engine.Utils.NBT;

public class NBT : TAG_Collection
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

    private TAG_Compound? base_Tag = null;

    public NBT() { }

    public NBT(string Compound_Name)
    {
        base_Tag = new() { Name = Compound_Name };
    }

    public T? TryGetTag<T>(string Tag_Name)
        where T : TAG_Base
    {
        if (base_Tag == null)
        {
            return null;
        }
        if (base_Tag.Name == Tag_Name && base_Tag is T matchedTag)
        {
            return matchedTag;
        }
        return base_Tag.TryGetTag<T>(Tag_Name);
    }

    public void WriteTag<T>(T Tag)
        where T : TAG_Base
    {
        if (base_Tag == null)
        {
            base_Tag = new() { Name = "" };
        }
        base_Tag.WriteTag<T>(Tag);
    }

    public bool RemoveTag(string Tag_Name)
    {
        if (base_Tag == null)
        {
            return false;
        }
        return base_Tag.RemoveTag(Tag_Name);
    }

    public bool ReadFromBytes(byte[] bytes, bool networkBytes = false)
    {
        base_Tag = new();
        bytes = base_Tag.ProcessBytes(bytes, networkBytes);
        return true;
    }

    public string toString2()
    {
        return (base_Tag != null) ? base_Tag.ToString() : "";
    }
}
