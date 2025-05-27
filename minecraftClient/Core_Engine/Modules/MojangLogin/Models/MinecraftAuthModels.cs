namespace Core_Engine.Modules.MojangLogin.MinecraftAuthModels
{
    public class MinecraftXSTSModel
    {
        public MinecraftXSTSModelInternals.PropertiesStruct Properties { get; set; }
        public string RelyingParty { get; set; }
        public string TokenType { get; set; }

        public MinecraftXSTSModel(string xbl_token, bool isBedrockRealms)
        {
            RelyingParty =
                (!isBedrockRealms)
                    ? "rp://api.minecraftservices.com/"
                    : "https://pocket.realms.minecraft.net/";
            TokenType = "JWT";
            Properties = new();
            Properties.SandboxId = "RETAIL";
            Properties.UserTokens = [xbl_token];
        }
    }

    public class MinecraftXSTSErrorResponse
    {
        public string Identity { get; set; }
        public int XErr { get; set; }
        public string Message { get; set; }
        public string Redirect { get; set; }
    }

    public class MinecraftAuthModel
    {
        public string identityToken { get; set; }

        public MinecraftAuthModel(string userHash, string xsts_token)
        {
            identityToken = $"XBL3.0 x={userHash};{xsts_token}";
        }
    }

    public class MinecraftAuthResponseModel
    {
        public string username { get; set; }
        public string[] roles { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }

    namespace MinecraftXSTSModelInternals
    {
        public class PropertiesStruct
        {
            public string SandboxId { get; set; }
            public string[] UserTokens { get; set; }
        }
    }
}
