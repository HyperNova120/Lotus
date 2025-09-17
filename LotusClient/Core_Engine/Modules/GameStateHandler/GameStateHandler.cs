using System.ComponentModel;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using LotusCore.BaseClasses;
using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.GameStateHandlerModule.BaseClasses;
using LotusCore.Modules.GameStateHandlerModule.Models;
using LotusCore.Modules.MojangLogin.MinecraftAuthModels;
using LotusCore.Modules.MojangLogin.Models;
using LotusCore.Utils;
using LotusCore.Utils.MinecraftPaths;
using Microsoft.AspNetCore.Identity;

namespace LotusCore.Modules.GameStateHandlerModule
{
    public class GameStateHandler : IModuleBase, IGameStateHandler
    {
        public GameStateHandler() { }

        private async Task HttpGetMojangKeyPair(MinecraftAuthResponseModel MinecraftAuth)
        {
            try
            {
                HttpRequestMessage requestMessage = HttpHandler.CreateHttpRequestMessage(
                    HttpMethod.Post,
                    $"{MojangAPIEndpoints._PlayerConfigEndpoints}/player/certificates",
                    null
                );
                requestMessage.Headers.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json")
                );
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    MinecraftAuth!.access_token
                );

                HttpResponseMessage response = await HttpHandler.SendRequest(requestMessage);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Logging.LogError(
                        $"Unable to get Mojang Key Pair: Code:{response.StatusCode.ToString()}"
                    );
                    return;
                }
                _MojangKeyPair = JsonSerializer.Deserialize<MojangKeyPair>(
                    await response.Content.ReadAsStringAsync()
                )!;

                Logging.LogDebug($"HttpGetMojangKeyPair:{_MojangKeyPair.expiresAt}");
            }
            catch (Exception e)
            {
                Logging.LogError($"GameStateHandler.HttpGetMojangKeyPair: {e}");
            }
        }

        //===========
        //IModuleBase
        //===========
        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

        public void RegisterEvents(Action<string> RegisterEvent) { }

        public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent)
        {
            SubscribeToEvent.Invoke(
                "MOJANGLOGIN_loginSuccessful",
                new EngineEventHandler(
                    (sender, args) =>
                    {
                        MojangLoginEventArgs EventArgs = (MojangLoginEventArgs)args;
                        _ = HttpGetMojangKeyPair(EventArgs._AuthModel);
                        _UserProfile = EventArgs._UserProfile;
                        return null;
                    }
                )
            );
        }

        //===========
        //IGameStateHandler
        //===========

        Dictionary<Identifier, ServerCookie> _ServerCookies = new();
        public Dictionary<Identifier, RegistryData> _ServerRegistryData = new();
        Dictionary<MinecraftUUID, ResourcePack> _ServerResourcePack = new();
        Dictionary<Identifier, List<ServerTag>> _ServerTags = new();
        HashSet<Identifier> _ServerFeatureFlags = new();

        DateTime _LastKeepAlivePacketTime = DateTime.Now;

        public MojangKeyPair _MojangKeyPair { get; private set; }
        public MinecraftProfile _UserProfile { get; private set; }

        public void AddServerCookie(ServerCookie cookie)
        {
            _ServerCookies[cookie._Key!] = cookie;
        }

        public void AddServerFeatureFlag(Identifier FeatureFlag)
        {
            _ServerFeatureFlags.Add(FeatureFlag);
        }

        public void UpdateServerRegistryData(
            RegistryData registryData,
            bool overwrite = true,
            bool replace = false
        )
        {
            //Logging.LogDebug($"UpdateServerRegistryData: {registryData._RegistryNameSpace}");
            if (!_ServerRegistryData.ContainsKey(registryData._RegistryNameSpace))
            {
                //new registry
                _ServerRegistryData[registryData._RegistryNameSpace] = registryData;
                return;
            }
            else if (replace)
            {
                _ServerRegistryData[registryData._RegistryNameSpace] = registryData;
                return;
            }
            RegistryData knownData = _ServerRegistryData[registryData._RegistryNameSpace];
            //update registry
            foreach (RegistryEntry newEntry in registryData._Entries)
            {
                int index = knownData._Entries.IndexOf(newEntry);
                if (index == -1)
                {
                    knownData._Entries.Add(new(newEntry));
                    continue;
                }
                else if (newEntry.Data == null)
                {
                    continue;
                }

                RegistryEntry currentEntry = knownData._Entries[index];
                if (currentEntry.Data == null)
                {
                    currentEntry.Data = newEntry.Data.Clone();
                    continue;
                }

                currentEntry.Data.Combine(newEntry.Data, overwrite);
            }
        }

        public void AddServerResourcePack(ResourcePack resourcePack)
        {
            _ServerResourcePack[resourcePack._UUID!] = resourcePack;
        }

        public void AddServerTag(Identifier Registry, Identifier TagName, List<int> Entries)
        {
            if (_ServerTags.ContainsKey(Registry))
            {
                foreach (var tmp in _ServerTags[Registry])
                {
                    if (tmp._TagName! == TagName)
                    {
                        //replace
                        tmp._Entries = Entries;
                        return;
                    }
                }
            }
            ServerTag serverTag = new() { _TagName = TagName, _Entries = Entries };
            _ServerTags[Registry] = [serverTag];
        }

        public ServerCookie? GetServerCookie(Identifier key)
        {
            return _ServerCookies.ContainsKey(key) ? _ServerCookies[key] : null;
        }

        public Identifier? GetServerFeatureFlag(Identifier FeatureFlag)
        {
            return _ServerFeatureFlags.Contains(FeatureFlag) ? FeatureFlag : null;
        }

        public RegistryData? GetServerRegistryData(Identifier ID)
        {
            return _ServerRegistryData.ContainsKey(ID) ? _ServerRegistryData[ID] : null;
        }

        public ResourcePack? GetServerResourcePack(MinecraftUUID ID)
        {
            return _ServerResourcePack.ContainsKey(ID) ? _ServerResourcePack[ID] : null;
        }

        public ServerTag? GetServerTag(Identifier Registry, Identifier TagName)
        {
            if (!_ServerTags.ContainsKey(Registry))
            {
                return null;
            }
            foreach (var tmp in _ServerTags[Registry])
            {
                if (tmp._TagName! == TagName)
                {
                    return tmp;
                }
            }
            return null;
        }

        public void ProcessTransfer()
        {
            _ServerRegistryData.Clear();
            _ServerFeatureFlags.Clear();
            _ServerResourcePack.Clear();
        }

        public void ProcessGameStateReset()
        {
            ProcessTransfer();
            _ServerCookies.Clear();
        }

        public void SetLastKeepAliveTime(DateTime lastPacketTime)
        {
            _LastKeepAlivePacketTime = lastPacketTime;
        }

        public DateTime GetLastKeepAliveTime()
        {
            return _LastKeepAlivePacketTime;
        }
    }
}
