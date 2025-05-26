using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;
using MinecraftAuthModels;
using Silk.NET.Vulkan;
using XboxLiveModels;

public static class MojangLogin
{
    private static IConfigurationRoot? configuration;

    private static AuthenticationResult? AuthResult;

    private static TokenAuthCert? XboxLiveAuth;

    private static TokenAuthCert? MinecraftXSTSCert;
    public static MinecraftAuthResponseModel? MinecraftAuth { get; private set; }

    public static MinecraftProfile? UserMinecraftProfile { get; private set; }

    public static async Task<bool> login(IConfigurationRoot config)
    {
        configuration = config;

        AuthResult = await GetUserAuth();
        if (AuthResult == null)
        {
            return false;
        }

        await AuthWithXboxLive(AuthResult.AccessToken);
        if (XboxLiveAuth == null)
        {
            return false;
        }

        await ObtainMinecraftXSTSToken();
        if (MinecraftXSTSCert == null)
        {
            return false;
        }

        await AuthWithMinecraft();
        if (MinecraftAuth == null)
        {
            return false;
        }

        if (!await CheckGameOwned())
        {
            Logging.LogInfo("Your account does not own Minecraft");
            return false;
        }

        if (!await GetUserMinecraftProfile())
        {
            Logging.LogError("Unable to get Minecraft profile");
            return false;
        }

        Logging.LogInfo($"Successfully logged you in, {UserMinecraftProfile!.name}");

        return true;
    }

    private static async Task<bool> GetUserMinecraftProfile()
    {
        HttpRequestMessage msg = HttpHandler.CreateHttpRequestMessage(
            HttpMethod.Get,
            "https://api.minecraftservices.com/minecraft/profile",
            null
        );
        msg.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            MinecraftAuth!.access_token
        );
        HttpResponseMessage response = await HttpHandler.SendRequest(msg);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            Logging.LogInfo("Minecraft Game Ownership Check Fail");
            return false;
        }

        UserMinecraftProfile = JsonSerializer.Deserialize<MinecraftProfile>(
            await response.Content.ReadAsStringAsync()
        );

        return true;
    }

    private static async Task<bool> CheckGameOwned()
    {
        HttpRequestMessage msg = HttpHandler.CreateHttpRequestMessage(
            HttpMethod.Get,
            "https://api.minecraftservices.com/entitlements/mcstore",
            null
        );
        msg.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            MinecraftAuth!.access_token
        );

        HttpResponseMessage response = await HttpHandler.SendRequest(msg);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            Logging.LogInfo("Minecraft Game Ownership Check Fail");
            return false;
        }
        //Logging.LogInfo("Owner Check:" + await response.Content.ReadAsStringAsync());
        return true;
    }

    private static async Task AuthWithMinecraft()
    {
        MinecraftAuthModel authModel = new MinecraftAuthModel(
            XboxLiveAuth!.DisplayClaims.xui[0].uhs,
            MinecraftXSTSCert!.Token
        );

        HttpRequestMessage msg = HttpHandler.CreateHttpRequestMessage(
            HttpMethod.Post,
            "https://api.minecraftservices.com/authentication/login_with_xbox",
            new StringContent(
                JsonSerializer.Serialize(authModel),
                Encoding.UTF8,
                "application/json"
            )
        );

        msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response = await HttpHandler.SendRequest(msg);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            Logging.LogInfo("Minecraft Authentication Failed");
            return;
        }
        MinecraftAuth = JsonSerializer.Deserialize<MinecraftAuthResponseModel>(
            await response.Content.ReadAsStringAsync()
        );
    }

    private static async Task ObtainMinecraftXSTSToken()
    {
        MinecraftXSTSModel authModel = new MinecraftXSTSModel(XboxLiveAuth!.Token, false);
        HttpRequestMessage msg = HttpHandler.CreateHttpRequestMessage(
            HttpMethod.Post,
            "https://xsts.auth.xboxlive.com/xsts/authorize",
            new StringContent(
                JsonSerializer.Serialize(authModel),
                Encoding.UTF8,
                "application/json"
            )
        );
        msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response = await HttpHandler.SendRequest(msg);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            Logging.LogInfo(
                "Minecraft XSTS Failed Error Code:"
                    + JsonSerializer.Deserialize<MinecraftXSTSErrorResponse>(
                        await response.Content.ReadAsStringAsync()
                    )
            );
            return;
        }
        MinecraftXSTSCert = JsonSerializer.Deserialize<TokenAuthCert>(
            await response.Content.ReadAsStringAsync()
        );
    }

    private static async Task AuthWithXboxLive(string accessToken)
    {
        XboxLiveAuthModel authModel = new XboxLiveAuthModel(accessToken);
        HttpRequestMessage msg = HttpHandler.CreateHttpRequestMessage(
            HttpMethod.Post,
            "https://user.auth.xboxlive.com/user/authenticate",
            new StringContent(
                JsonSerializer.Serialize(authModel),
                Encoding.UTF8,
                "application/json"
            )
        );

        msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response = await HttpHandler.SendRequest(msg);
        var responsetmp = await response.Content.ReadAsStringAsync();

        if (response.StatusCode != HttpStatusCode.OK)
        {
            Logging.LogInfo("XboxLive Authentication Failed");
            return;
        }
        XboxLiveAuth = JsonSerializer.Deserialize<TokenAuthCert>(responsetmp);

        await Task.Delay(1);
    }

    private static async Task<AuthenticationResult?> GetUserAuth()
    {
        var scopes = new[] { "XboxLive.signin" };

        BrokerOptions options = new BrokerOptions(
            BrokerOptions.OperatingSystems.Windows | BrokerOptions.OperatingSystems.Linux
        );
        options.Title = "Custom Minecraft Client";

        IPublicClientApplication app = PublicClientApplicationBuilder
            .Create((string)configuration!["AzureApp:AppID"]!)
            .WithDefaultRedirectUri()
            .WithParentActivityOrWindow(GetConsoleOrTerminalWindow)
            .WithBroker(options)
            .Build();

        var accounts = await app.GetAccountsAsync();
        var existingAccount = accounts.FirstOrDefault();
        try
        {
            try
            {
                return await app.AcquireTokenSilent(scopes, existingAccount).ExecuteAsync();
            }
            catch (MsalUiRequiredException ex)
            {
                //Logging.LogDebug(ex.ToJsonString());
                if (ConsoleUtils.AskUserYNQuestion("Would you like to log in?"))
                {
                    return await app.AcquireTokenInteractive(scopes)
                        .WithParentActivityOrWindow(GetConsoleOrTerminalWindow())
                        .ExecuteAsync();
                }
                else
                {
                    return null;
                }
            }
        }
        catch
        {
            Logging.LogError("Login Failed");
            return null;
        }
    }

    enum GetAncestorFlags
    {
        GetParent = 1,
        GetRoot = 2,

        /// <summary>
        /// Retrieves the owned root window by walking the chain of parent and owner windows returned by GetParent.
        /// </summary>
        GetRootOwner = 3,
    }

    /// <summary>
    /// Retrieves the handle to the ancestor of the specified window.
    /// </summary>
    /// <param name="hwnd">A handle to the window whose ancestor is to be retrieved.
    /// If this parameter is the desktop window, the function returns NULL. </param>
    /// <param name="flags">The ancestor to be retrieved.</param>
    /// <returns>The return value is the handle to the ancestor window.</returns>
    [DllImport("user32.dll", ExactSpelling = true)]
    static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    // This is your window handle!
    public static IntPtr GetConsoleOrTerminalWindow()
    {
        IntPtr consoleHandle = GetConsoleWindow();
        IntPtr handle = GetAncestor(consoleHandle, GetAncestorFlags.GetRootOwner);

        return handle;
    }
}
