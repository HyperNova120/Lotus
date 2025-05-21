
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public static class Login
{
    private static IConfigurationRoot configuration;
    public static readonly string redirect_uri = "http://localhost:8080";
    static string API_Endpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize";
    static string scope = "XboxLive.signin";
    static Random random = new Random();
    static long XSRF_State;

    static string code_verifier;
    static string code_challenge;

    static string Post_Resource = "/consumers/oauth2/v2.0/token HTTP/1.1";
    static string Post_Host = "https://login.microsoftonline.com";

    public static async Task<bool> login(IConfigurationRoot config)
    {
        configuration = config;
        HttpServer.openServer();
        string? code = SendAuthCodeRequest();
        if (code == null)
        {
            Logging.LogDebug("Auth Code Response Invalid");
            HttpServer.closeServer();
            return false;
        }
        Logging.LogDebug("Posting Request");
        string? accessToken = await SendAccessTokenRequest(code);
        if (accessToken == null)
        {
            Logging.LogDebug("Access Token Request Failed");
            HttpServer.closeServer();
            return false;
        }   
        Logging.LogDebug(accessToken!);
        HttpServer.closeServer();
        return true;
    }

    private static async Task<string?> SendAccessTokenRequest(string code)
    {
        
        var values = new Dictionary<string, string>
        {
            {"client_id", (string)configuration["AzureApp:AppID"]!},
            {"scope",scope},
            {"code",code},
            {"redirect_uri", redirect_uri},
            {"grant_type", "authorization_code"},
            {"code_verifier", code_verifier}
        };
        var content = new FormUrlEncodedContent(values);




        /* string clientID = $"client_id={(string)configuration["AzureApp:AppID"]!}";
        string codeURL = $"code={code}";
        string redirectURI = $"redirect_uri={redirect_uri}";
        string grant_type = $"grant_type=authorization_code";
        string codeVerifier = $"code_verifier={code_verifier}";
        string scopeURL = $"scope={scope}";

        var request = new HttpRequestMessage(HttpMethod.Post, Post_Host + Post_Resource);
        request.Content = new StringContent($"{clientID}&{scopeURL}&{codeURL}&{redirectURI}&{grant_type}&{codeVerifier}", Encoding.UTF8, "application/x-www-form-urlencoded");

        //request.Headers.Add("Host", Post_Host);
        //request.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

        Console.WriteLine($"{request.Method} {request.RequestUri}");
        Console.WriteLine($"Headers: {request.Headers}");
        Console.WriteLine($"Body: {await request.Content.ReadAsStringAsync()}"); */

        _ = HttpServer.PostRequest(Post_Host + Post_Resource, content);

        var serverResult = HttpServer.GetQueries();
        foreach (string key in serverResult.AllKeys)
        {
            Logging.LogDebug($"KEY:{key} VALUE:{serverResult[key]}");
        }


        return null;
    }

    private static string? SendAuthCodeRequest()
    {
        GenerateCodeVerifier();
        GenerateCodeChallenge();
        string hex_state = GenerateXSRFState();
        string url = $"{API_Endpoint}?client_id={configuration["AzureApp:AppID"]}&response_type=code&redirect_uri={UrlEncoder.Default.Encode(redirect_uri)}&response_mode=query&scope={UrlEncoder.Default.Encode(scope)}&state={UrlEncoder.Default.Encode(hex_state)}&code_challenge={code_challenge}&code_challenge_method=S256";
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
        var response = HttpServer.GetQueries();
        foreach (string key in response.AllKeys)
        {
            Logging.LogDebug($"KEY:{key} VALUE:{response[key]}");
        }
        if (response == null || !response.AllKeys.Contains("state") || !response.AllKeys.Contains("code") || (response.AllKeys.Contains("state") && (string)response["state"]! != hex_state))
        {
            if (response.AllKeys.Contains("error"))
            {
                Logging.LogError($"Auth Fail: ERROR:{(string)response["error"]!} DESCRIPTION:{(string)response["error_description"]!}");
            }
            return null;
        }

        return (string)response["code"]!;
    }

    private static string GenerateXSRFState()
    {
        XSRF_State = random.NextInt64();
        return Convert.ToHexString(BitConverter.GetBytes(XSRF_State));
    }

    private static void GenerateCodeVerifier()
    {
        var rng = RandomNumberGenerator.Create();
        byte[] bytes = new byte[42];
        rng.GetBytes(bytes);
        code_verifier = Base64UrlEncode(bytes);
    }

    private static void GenerateCodeChallenge()
    {
        var sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(code_verifier));
        code_challenge = Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}