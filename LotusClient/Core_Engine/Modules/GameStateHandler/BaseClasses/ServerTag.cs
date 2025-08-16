using Core_Engine.BaseClasses;

namespace Core_Engine.Modules.GameStateHandler.BaseClasses;

public class ServerTag
{
    public Identifier? TagName;
    public List<int> Entries = new();
}
