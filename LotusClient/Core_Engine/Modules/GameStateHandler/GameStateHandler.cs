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

        Dictionary<Identifier, ServerCookie> serverCookies = new();
        public Dictionary<Identifier, RegistryData> serverRegistryData = new();
        Dictionary<MinecraftUUID, ResourcePack> serverResourcePack = new();
        HashSet<Identifier> serverFeatureFlags = new();

        public void AddServerCookie(ServerCookie cookie)
        {
            serverCookies[cookie.Key!] = cookie;
        }

        public void AddServerFeatureFlag(Identifier FeatureFlag)
        {
            serverFeatureFlags.Add(FeatureFlag);
        }

        public void AddServerRegistryData(RegistryData registryData)
        {
            
            serverRegistryData[registryData.ID!] = registryData;
        }

        public void AddServerResourcePack(ResourcePack resourcePack)
        {
            serverResourcePack[resourcePack.UUID!] = resourcePack;
        }

        public void AddServerTag(Identifier Registry, Identifier TagName)
        {
            throw new NotImplementedException();
        }

        public ServerCookie? GetServerCookie(Identifier key)
        {
            return serverCookies.ContainsKey(key) ? serverCookies[key] : null;
        }

        public Identifier? GetServerFeatureFlag(Identifier FeatureFlag)
        {
            return serverFeatureFlags.Contains(FeatureFlag) ? FeatureFlag : null;
        }

        public RegistryData? GetServerRegistryData(Identifier ID)
        {
            return serverRegistryData.ContainsKey(ID) ? serverRegistryData[ID] : null;
        }

        public ResourcePack? GetServerResourcePack(MinecraftUUID ID)
        {
            return serverResourcePack.ContainsKey(ID) ? serverResourcePack[ID] : null;
        }

        public ServerTag? GetServerTag(Identifier Registry, Identifier TagName)
        {
            throw new NotImplementedException();
        }

        public void ProcessTransfer()
        {
            serverRegistryData.Clear();
            serverFeatureFlags.Clear();
            serverResourcePack.Clear();
        }

        public void ProcessGameStateReset()
        {
            ProcessTransfer();
            serverCookies.Clear();
        }
    }
}
