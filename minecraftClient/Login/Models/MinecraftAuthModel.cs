public class MinecraftAuthModel
{
    public PropertiesStruct Properties { get; set; }
    public string RelyingParty { get; set; }
    public string TokenType { get; set; }

    public MinecraftAuthModel(string xbl_token, bool isBedrockRealms)
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

public class PropertiesStruct
{
    public string SandboxId { get; set; }
    public string[] UserTokens { get; set; }
}

public class MinecraftAuthErrorResponse
{
    public string Identity { get; set; }
    public int XErr { get; set; }
    public string Message { get; set; }
    public string Redirect { get; set; }
}
