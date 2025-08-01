using Core_Engine.Utils.NBT.Tags;

namespace Core_Engine.Utils.NBT.BaseClasses;

public interface TAG_Collection
{
    public T? TryGetTag<T>(string Tag_Name)
        where T : TAG_Base;

    public void WriteTag<T>(T Tag)
        where T : TAG_Base;

    public bool RemoveTag(string Tag_Name);

    public TAG_Compound? TryGetCompountTag(string Tag_Name)
    {
        return TryGetTag<TAG_Compound>(Tag_Name);
    }

    public TAG_List? TryGetListTag(string Tag_Name)
    {
        return TryGetTag<TAG_List>(Tag_Name);
    }
}
