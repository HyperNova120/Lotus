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
using Silk.NET.Vulkan;

public static class Login
{
    private static IConfigurationRoot configuration;
    public static readonly string redirect_uri = "http://localhost:8080";

    private static AuthenticationResult AuthResult;

    private static TokenAuthCert? XboxLiveAuth;

    public static async Task<bool> login(IConfigurationRoot config)
    {
        configuration = config;

        AuthResult = await GetUserAuth();
        /* Logging.LogDebug(
            $"AuthResult: \n\tAccount:{AuthResult.Account}\n\tExpires On:{AuthResult.ExpiresOn}\n\tAccess Token:{AuthResult.AccessToken}"
        ); */
        await AuthWithXboxLive(AuthResult.AccessToken);

        return true;
    }

    private static async Task AuthWithXboxLive(string accessToken)
    {
        HttpRequestMessage msg = new HttpRequestMessage();
        msg.Method = HttpMethod.Post;
        msg.RequestUri = new Uri("https://user.auth.xboxlive.com/user/authenticate");
        msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        XboxLiveAuthModel tmp = new XboxLiveAuthModel(accessToken);

        string serializedContent = JsonSerializer.Serialize(tmp);
        //Logging.LogDebug("HTTP REQUEST:\n" + serializedContent);

        msg.Content = new StringContent(serializedContent, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await HttpServer.SendRequest(msg);
        //Logging.LogDebug("HTTP Response:\n" + response.ToString());
        var responsetmp = await response.Content.ReadAsStringAsync();
        //Logging.LogDebug("HTTP Content:\n" + responsetmp);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            Logging.LogInfo("XboxLive Authentication Failed");
            return;
        }
        XboxLiveAuth = JsonSerializer.Deserialize<TokenAuthCert>(responsetmp);

        await Task.Delay(1);
    }

    private static async Task<AuthenticationResult> GetUserAuth()
    {
        var scopes = new[] { "XboxLive.signin" };

        BrokerOptions options = new BrokerOptions(
            BrokerOptions.OperatingSystems.Windows | BrokerOptions.OperatingSystems.Linux
        );
        options.Title = "Custom Minecraft Client";

        IPublicClientApplication app = PublicClientApplicationBuilder
            .Create((string)configuration["AzureApp:AppID"]!)
            .WithDefaultRedirectUri()
            .WithParentActivityOrWindow(GetConsoleOrTerminalWindow)
            .WithBroker(options)
            .Build();

        var accounts = await app.GetAccountsAsync();
        var existingAccount = accounts.FirstOrDefault();
        try
        {
            return await app.AcquireTokenSilent(scopes, existingAccount).ExecuteAsync();
        }
        catch (MsalUiRequiredException ex)
        {
            return await app.AcquireTokenInteractive(scopes).ExecuteAsync();
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
