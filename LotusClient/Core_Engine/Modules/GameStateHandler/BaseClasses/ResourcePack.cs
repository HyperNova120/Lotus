using Core_Engine.BaseClasses;
using Core_Engine.BaseClasses.Types;

namespace Core_Engine.Modules.GameStateHandler.BaseClasses
{
    public class ResourcePack
    {
        public MinecraftUUID? UUID;
        public string? URL;
        public string? Hash;
        public bool Forced;
        public string? TextComponent;
    }
}
