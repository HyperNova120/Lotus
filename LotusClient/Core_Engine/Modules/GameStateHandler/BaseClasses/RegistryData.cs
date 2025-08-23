using System.Diagnostics.CodeAnalysis;
using LotusCore.BaseClasses;
using LotusCore.Utils;

namespace LotusCore.Modules.GameStateHandler.BaseClasses
{
    public class RegistryData
    {
        public Identifier _RegistryNameSpace;
        public List<RegistryEntry> _Entries;

        public string? _Version = null;

        public RegistryData()
        {
            _Entries = new();
            _RegistryNameSpace = null;
        }
    }

    public class RegistryEntry
    {
        public Identifier ID;

        public NBT? Data;

        public RegistryEntry() { }

        public RegistryEntry(RegistryEntry other)
        {
            ID = new Identifier(other.ID);
            if (other.Data != null)
            {
                Data = other.Data.Clone();
            }
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is RegistryEntry re && re.ID == ID;
        }

        public static bool operator ==(RegistryEntry a, RegistryEntry b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;
            return a.ID == b.ID;
        }

        public static bool operator !=(RegistryEntry a, RegistryEntry b) => !(a == b);

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, Data);
        }
    }
}
