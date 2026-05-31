using LotusCore.Utils.NBTInternals.Tags;

namespace LotusCore.Utils.NBTInternals.BaseClasses;

public interface TAG_Collection
{
    public TAG_Base? TryGetTag(string Tag_Name);

    public void WriteTag<T>(T Tag)
        where T : TAG_Base;

    public bool RemoveTag(string Tag_Name);

    public TAG_Compound? TryGetCompountTag(string Tag_Name)
    {
        TAG_Base? tmp = TryGetTag(Tag_Name);
        if (tmp == null || (tmp != null && tmp._Type_ID != (int)TAG_Base.TagTypeID.TAG_COMPOUND))
        {
            return null;
        }
        return (TAG_Compound)tmp!;
    }

    public TAG_List? TryGetListTag(string Tag_Name)
    {
        TAG_Base? tmp = TryGetTag(Tag_Name);
        if (tmp == null || (tmp != null && tmp._Type_ID != 9))
        {
            return null;
        }
        return (TAG_List)tmp!;
    }
}
