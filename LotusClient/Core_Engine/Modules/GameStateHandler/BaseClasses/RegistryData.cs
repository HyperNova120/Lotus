using Core_Engine.BaseClasses;
using Core_Engine.Utils;

namespace Core_Engine.Modules.GameStateHandler.BaseClasses
{
    public class RegistryData
    {
        public Identifier? _RegistryNameSpace;
        public List<RegistryEntry> _Entries;

        public string? _Version = null;

        public RegistryData()
        {
            _Entries = new();
        }
    }

    public struct RegistryEntry
    {
        public Identifier ID;

        public NBT Data;
    }
}
