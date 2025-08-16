using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Core_Engine.Interfaces;
using Core_Engine.Modules.MojangLogin.Commands;
using Core_Engine.Modules.MojangLogin.Internals;
using Core_Engine.Modules.MojangLogin.MinecraftAuthModels;
using Core_Engine.Modules.MojangLogin.Models;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;

namespace Core_Engine.Modules.MojangLogin
{
    public class MojangLogin : IModuleBase
    {
        public MinecraftProfile? userProfile { get; private set; } = null;
        public MinecraftAuthResponseModel? MinecraftAuth { get; private set; } = null;

        private readonly MojangLoginInternals Internals = new();

        public void RegisterEvents(Action<string> RegisterEvent) { }

        public void SubscribeToEvents(Action<string, EventHandler> SubscribeToEvent) { }

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand)
        {
            RegisterCommand.Invoke("login", new LoginCommand());
        }

        public async Task<bool> LoginAsync()
        {
            AuthenticationResult? AuthResult = await Internals.GetUserAuth();
            if (AuthResult == null)
            {
                Core_Engine.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                return false;
            }

            TokenAuthCert? XboxLiveAuth = await Internals.AuthWithXboxLive(AuthResult.AccessToken);
            if (XboxLiveAuth == null)
            {
                Core_Engine.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                return false;
            }

            TokenAuthCert? MinecraftXSTSCert = await Internals.ObtainMinecraftXSTSToken(
                XboxLiveAuth
            );
            if (MinecraftXSTSCert == null)
            {
                Core_Engine.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                return false;
            }

            MinecraftAuth = await Internals.AuthWithMinecraft(XboxLiveAuth, MinecraftXSTSCert);
            if (MinecraftAuth == null)
            {
                Core_Engine.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                return false;
            }

            if (!await Internals.CheckGameOwned(MinecraftAuth))
            {
                Logging.LogInfo("Your account does not own Minecraft");
                Core_Engine.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                return false;
            }

            userProfile = await Internals.GetUserMinecraftProfile(MinecraftAuth);
            if (userProfile == null)
            {
                Logging.LogError("Unable to get Minecraft profile");
                Core_Engine.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                return false;
            }

            Core_Engine.SignalInteractiveFree(Core_Engine.State.AccountLogin);
            return true;
        }
    }
}
