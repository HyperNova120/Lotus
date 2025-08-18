using System.Reflection.Metadata.Ecma335;
using Core_Engine.BaseClasses;
using Core_Engine.Interfaces;
using Core_Engine.Modules.GameStateHandler.BaseClasses;

namespace Core_Engine.Modules.GameStateHandler
{
    public class GameStateHandler : IModuleBase, IGameStateHandler
    {
        public GameStateHandler() { }

        //===========
        //IModuleBase
        //===========
        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

        public void RegisterEvents(Action<string> RegisterEvent) { }

        public void SubscribeToEvents(Action<string, EventHandler> SubscribeToEvent) { }

        //===========
        //IGameStateHandler
        //===========

        Dictionary<Identifier, ServerCookie> _ServerCookies = new();
        public Dictionary<Identifier, RegistryData> _ServerRegistryData = new();
        Dictionary<MinecraftUUID, ResourcePack> _ServerResourcePack = new();
        HashSet<Identifier> _ServerFeatureFlags = new();

        public void AddServerCookie(ServerCookie cookie)
        {
            _ServerCookies[cookie._Key!] = cookie;
        }

        public void AddServerFeatureFlag(Identifier FeatureFlag)
        {
            _ServerFeatureFlags.Add(FeatureFlag);
        }

        public void AddServerRegistryData(RegistryData registryData)
        {
            _ServerRegistryData[registryData._RegistryNameSpace!] = registryData;
        }

        public void AddServerResourcePack(ResourcePack resourcePack)
        {
            _ServerResourcePack[resourcePack._UUID!] = resourcePack;
        }

        public void AddServerTag(Identifier Registry, Identifier TagName)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
    }
}
