namespace Core_Engine.Modules.MojangLogin.Models
{
    public class MinecraftProfile
    {
        public string id { get; set; } = "";
        public string name { get; set; } = "";
        public MinecraftProfileInternals.MinecraftSkin[] skins { get; set; } = [];
        public MinecraftProfileInternals.MinecraftCape[] capes { get; set; } = [];
    }

    namespace MinecraftProfileInternals
    {
        public class MinecraftSkin
        {
            public string id { get; set; } = "";
            public string state { get; set; } = "";
            public string url { get; set; } = "";
            public string variant { get; set; } = "";
            public string alias { get; set; } = "";
        }

        public class MinecraftCape
        {
            public string id { get; set; } = "";
            public string state { get; set; } = "";
            public string url { get; set; } = "";
            public string alias { get; set; } = "";
        }
    }
}
