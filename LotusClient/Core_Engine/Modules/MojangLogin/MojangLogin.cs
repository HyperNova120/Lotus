using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using LotusCore.Interfaces;
using LotusCore.Modules.MojangLogin.Commands;
using LotusCore.Modules.MojangLogin.Internals;
using LotusCore.Modules.MojangLogin.MinecraftAuthModels;
using LotusCore.Modules.MojangLogin.Models;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;

namespace LotusCore.Modules.MojangLogin
{
    public class MojangLogin : IModuleBase
    {
        public MinecraftProfile? _UserProfile { get; private set; } = null;
        public MinecraftAuthResponseModel? _MinecraftAuth { get; private set; } = null;

        private readonly MojangLoginInternals _Internals = new();

        public void RegisterEvents(Action<string> RegisterEvent) { }

        public void SubscribeToEvents(Action<string, EventHandler> SubscribeToEvent) { }

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand)
        {
            RegisterCommand.Invoke("login", new LoginCommand());
        }

        public async Task<bool> LoginAsync()
        {
            try
            {
                AuthenticationResult? AuthResult = await _Internals.GetUserAuth();
                if (AuthResult == null)
                {
                    Core_Engine.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                    Logging.LogError("Auth Fail", true);
                    return false;
                }

                TokenAuthCert? XboxLiveAuth = await _Internals.AuthWithXboxLive(
                    AuthResult.AccessToken
                );
                if (XboxLiveAuth == null)
                {
                    Core_Engine.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                    Logging.LogError("Auth Fail", true);
                    return false;
                }

                TokenAuthCert? MinecraftXSTSCert = await _Internals.ObtainMinecraftXSTSToken(
                    XboxLiveAuth
                );
                if (MinecraftXSTSCert == null)
                {
                    Core_Engine.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                    Logging.LogError("Auth Fail", true);
                    return false;
                }

                _MinecraftAuth = await _Internals.AuthWithMinecraft(
                    XboxLiveAuth,
                    MinecraftXSTSCert
                );
                if (_MinecraftAuth == null)
                {
                    Core_Engine.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                    Logging.LogError("Auth Fail", true);
                    return false;
                }

                if (!await _Internals.CheckGameOwned(_MinecraftAuth))
                {
                    Logging.LogInfo("Your account does not own Minecraft");
                    Core_Engine.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                    Logging.LogError("Auth Fail", true);
                    return false;
                }

                _UserProfile = await _Internals.GetUserMinecraftProfile(_MinecraftAuth);
                if (_UserProfile == null)
                {
                    Logging.LogError("Unable to get Minecraft profile");
                    Core_Engine.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                    Logging.LogError("Auth Fail", true);
                    return false;
                }

                Core_Engine.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                return true;
            }
            catch (Exception e)
            {
                Logging.LogError(e.ToString());
                return false;
            }
        }
    }
}
