using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Core_Engine.Modules.MojangLogin.MinecraftAuthModels;
using Core_Engine.Modules.MojangLogin.Models;
using Core_Engine.Modules.Networking.Internals;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;

namespace Core_Engine.Modules.MojangLogin.Internals
{
    public class MojangLoginInternals
    {
        public async Task<AuthenticationResult?> GetUserAuth()
        {
            var scopes = new[] { "XboxLive.signin" };
            BrokerOptions options = new BrokerOptions(
                BrokerOptions.OperatingSystems.Windows | BrokerOptions.OperatingSystems.Linux
            );
            options.Title = "Lotus Client";

            IPublicClientApplication app = PublicClientApplicationBuilder
                .Create(Environment.GetEnvironmentVariable("AppID"))
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
                    return await app.AcquireTokenInteractive(scopes)
                        .WithParentActivityOrWindow(GetConsoleOrTerminalWindow())
                        .ExecuteAsync();
                }
            }
            catch
            {
                Logging.LogError("Login Failed");
                return null;
            }
        }

        public async Task<TokenAuthCert?> AuthWithXboxLive(string accessToken)
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
                return null;
            }
            return JsonSerializer.Deserialize<TokenAuthCert>(responsetmp)!;
        }

        public async Task<TokenAuthCert?> ObtainMinecraftXSTSToken(TokenAuthCert XboxLiveAuth)
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
                return null;
            }
            return JsonSerializer.Deserialize<TokenAuthCert>(
                await response.Content.ReadAsStringAsync()
            );
        }

        public async Task<MinecraftAuthResponseModel?> AuthWithMinecraft(
            TokenAuthCert XboxLiveAuth,
            TokenAuthCert MinecraftXSTSCert
        )
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
                Logging.LogInfo(
                    "Minecraft Authentication Failed: " + await response.Content.ReadAsStringAsync()
                );
                return null;
            }
            return JsonSerializer.Deserialize<MinecraftAuthResponseModel>(
                await response.Content.ReadAsStringAsync()
            );
        }

        public async Task<bool> CheckGameOwned(MinecraftAuthResponseModel MinecraftAuth)
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
                return false;
            }
            //Logging.LogInfo("Owner Check:" + await response.Content.ReadAsStringAsync());
            return true;
        }

        public async Task<MinecraftProfile?> GetUserMinecraftProfile(
            MinecraftAuthResponseModel MinecraftAuth
        )
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
                return null;
            }

            return JsonSerializer.Deserialize<MinecraftProfile>(
                await response.Content.ReadAsStringAsync()
            );
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
}
