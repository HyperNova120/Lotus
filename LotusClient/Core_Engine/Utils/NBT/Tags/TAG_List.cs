using Core_Engine.Utils.NBT.BaseClasses;

namespace Core_Engine.Utils.NBT.Tags;

public class TAG_List : TAG_Base, TAG_Collection
{
    NBT.NBT_Tags Tag_Type;

    int length;

    List<TAG_Base> Contained_Tags;

    public override byte[] ProcessBytes(byte[] inputBytes)
    {
        throw new NotImplementedException();
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

    public T? TryGetTag<T>(string Tag_Name)
        where T : TAG_Base
    {
        foreach (TAG_Base tag in Contained_Tags)
        {
            if (tag.Name == Tag_Name && tag is T)
            {
                return (T)tag;
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
                //replace
                Contained_Tags[i] = Tag;
                return;
            }
        }
        Contained_Tags.Add(Tag);
    }
}
