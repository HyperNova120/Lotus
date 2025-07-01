namespace Core_Engine.Utils.NBT.BaseClasses;

public abstract class TAG_Base
{
    public int Type_ID;
    public string Name;

    public abstract byte[] ProcessBytes(byte[] inputBytes);
}
