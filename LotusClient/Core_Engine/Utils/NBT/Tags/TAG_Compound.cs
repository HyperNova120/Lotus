using Core_Engine.Utils.NBT.BaseClasses;

namespace Core_Engine.Utils.NBT.Tags;

public class TAG_Compound : TAG_Base, TAG_Collection
{
    private List<TAG_Base> Contained_Tags = new();

    public override byte[] ProcessBytes(byte[] inputBytes)
    {
        Type_ID = (int)NBT.NBT_Tags.TAG_Compound;
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
        foreach (TAG_Base Tag in Contained_Tags)
        {
            if (Tag.Name == Tag_Name && Tag is T)
            {
                return (T)Tag;
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
                Contained_Tags[i] = Tag;
                return;
            }
        }
        Contained_Tags.Add(Tag);
    }
}
