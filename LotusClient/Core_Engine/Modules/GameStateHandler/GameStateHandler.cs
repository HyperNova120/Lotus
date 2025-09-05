using System.Reflection.Metadata.Ecma335;
using LotusCore.BaseClasses;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.GameStateHandler.BaseClasses;

namespace LotusCore.Modules.GameStateHandler
{
    public class GameStateHandler : IModuleBase, IGameStateHandler
    {
        public GameStateHandler() { }

        //===========
        //IModuleBase
        //===========
        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

        public void RegisterEvents(Action<string> RegisterEvent) { }

        public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent) { }

        //===========
        //IGameStateHandler
        //===========

        Dictionary<Identifier, ServerCookie> _ServerCookies = new();
        public Dictionary<Identifier, RegistryData> _ServerRegistryData = new();
        Dictionary<MinecraftUUID, ResourcePack> _ServerResourcePack = new();
        HashSet<Identifier> _ServerFeatureFlags = new();

        DateTime _LastKeepAlivePacketTime = DateTime.Now;

        public void AddServerCookie(ServerCookie cookie)
        {
            _ServerCookies[cookie._Key!] = cookie;
        }

        public void AddServerFeatureFlag(Identifier FeatureFlag)
        {
            _ServerFeatureFlags.Add(FeatureFlag);
        }

        public void UpdateServerRegistryData(RegistryData registryData, bool overwrite = true)
        {
            if (!_ServerRegistryData.ContainsKey(registryData._RegistryNameSpace))
            {
                //new registry
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
