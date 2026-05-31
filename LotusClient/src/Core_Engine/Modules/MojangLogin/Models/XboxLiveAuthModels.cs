namespace LotusCore.Modules.MojangLogin.Models
{
    public class XboxLiveAuthModel
    {
        public Dictionary<string, string> Properties { get; set; }
        public string RelyingParty { get; set; }
        public string TokenType { get; set; }

        public XboxLiveAuthModel(string accessToken)
        {
            RelyingParty = "http://auth.xboxlive.com";
            TokenType = "JWT";
            Properties = new()
            {
                { "AuthMethod", "RPS" },
                { "SiteName", "user.auth.xboxlive.com" },
                { "RpsTicket", $"d={accessToken}" },
            };
        }
    }

    public class TokenAuthCert
    {
        public DateTime IssueInstant { get; set; }
        public DateTime NotAfter { get; set; }
        public string Token { get; set; } = "";
        public TokenAuthCertInternals.DisplayClaims DisplayClaims { get; set; } = new();
    }

    namespace TokenAuthCertInternals
    {
        public class DisplayClaims
        {
            public Xui[] xui { get; set; } = [];
        }

        public class Xui
        {
            public string uhs { get; set; } = "";
        }
    }
}
