using Core_Engine.BaseClasses;
using Core_Engine.Utils;

namespace Core_Engine.Modules.GameStateHandler.BaseClasses
{
    public class RegistryData
    {
        public Identifier? ID;
        public List<RegistryEntry> Entries;

        public RegistryData()
        {
            Entries = new();
        }
    }

    public struct RegistryEntry
    {
        public Identifier ID;

        public NBT Data;
    }
}
