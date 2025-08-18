using Core_Engine.BaseClasses;
using Core_Engine.BaseClasses.Types;

namespace Core_Engine.Modules.GameStateHandler.BaseClasses
{
    public class ResourcePack
    {
        public MinecraftUUID? _UUID;
        public string? _URL;
        public string? _Hash;
        public bool _Forced;
        public string? _TextComponent;
    }
}
