using Core_Engine.Utils.NBT.BaseClasses;
using Core_Engine.Utils.NBT.Tags;

namespace Core_Engine.Utils.NBT;

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

    private TAG_Compound? base_Tag;

    public NBT() { }

    public NBT(string Compound_Name)
    {
        base_Tag = new() { Name = Compound_Name };
    }
}
