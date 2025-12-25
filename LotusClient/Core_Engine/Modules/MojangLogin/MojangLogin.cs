using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.MojangLogin.Commands;
using LotusCore.Modules.MojangLogin.Internals;
using LotusCore.Modules.MojangLogin.MinecraftAuthModels;
using LotusCore.Modules.MojangLogin.Models;
using LotusCore.Modules.MojangLogin.Types;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;

namespace LotusCore.Modules.MojangLogin
{
    public class MojangLogin : IMojangLoginModule, IModuleBase
    {
        public MinecraftProfile? _userProfile { get; private set; } = null;
        public MinecraftAuthResponseModel? _minecraftAuth { get; private set; } = null;

        ICoreModule? _coreModule;

        private readonly MojangLoginInternals _internals = new();

        public void RegisterEvents(Action<string> RegisterEvent)
        {
            RegisterEvent.Invoke("MOJANGLOGIN_loginSuccessful");
        }

        public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent) { }

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand)
        {
            RegisterCommand.Invoke("login", new LoginCommand(this));
        }

        public void LinkModules(ICoreModule coreModule)
        {
            _coreModule = coreModule;
        }

        public async Task<bool> LoginAsync()
        {
            try
            {
                AuthenticationResult? AuthResult = await _internals.GetUserAuth();
                if (AuthResult == null)
                {
                    _coreModule!.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                    Logging.LogError("Auth Fail", true);
                    return false;
                }
                //Logging.LogDebug($"AuthResult.AccessToken:{AuthResult.AccessToken}");

                TokenAuthCert? XboxLiveAuth = await _internals.AuthWithXboxLive(
                    AuthResult.AccessToken
                );
                if (XboxLiveAuth == null)
                {
                    _coreModule!.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                    Logging.LogError("Auth Fail", true);
                    return false;
                }

                TokenAuthCert? MinecraftXSTSCert = await _internals.ObtainMinecraftXSTSToken(
                    XboxLiveAuth
                );
                if (MinecraftXSTSCert == null)
                {
                    _coreModule!.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                    Logging.LogError("Auth Fail", true);
                    return false;
                }

                _minecraftAuth = await _internals.AuthWithMinecraft(
                    XboxLiveAuth,
                    MinecraftXSTSCert
                );
                if (_minecraftAuth == null)
                {
                    _coreModule!.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                    Logging.LogError("Auth Fail", true);
                    return false;
                }

                if (!await _internals.CheckGameOwned(_minecraftAuth))
                {
                    Logging.LogInfo("Your account does not own Minecraft");
                    _coreModule!.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                    Logging.LogError("Auth Fail", true);
                    return false;
                }

                _userProfile = await _internals.GetUserMinecraftProfile(_minecraftAuth);
                if (_userProfile == null)
                {
                    Logging.LogError("Unable to get Minecraft profile");
                    _coreModule!.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                    Logging.LogError("Auth Fail", true);
                    return false;
                }

                _coreModule!.SignalInteractiveFree(Core_Engine.State.AccountLogin);
                _coreModule!.InvokeEvent(
                    "MOJANGLOGIN_loginSuccessful",
                    new MojangLoginEventArgs()
                    {
                        _AuthModel = _minecraftAuth,
                        _UserProfile = this._userProfile,
                    }
                );
                return true;
            }
            catch (Exception e)
            {
                Logging.LogError(e.ToString());
                return false;
            }
        }

        public MinecraftProfile? GetUserProfile()
        {
            return _userProfile;
        }

        public MinecraftAuthResponseModel? GetMinecraftAuth()
        {
            return _minecraftAuth;
        }
    }
}
