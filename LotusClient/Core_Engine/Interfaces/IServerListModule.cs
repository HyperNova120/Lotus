using System.Net;
using LotusCore.EngineEvents;
using LotusCore.Modules.LotusNetty;
using LotusCore.Modules.LotusNetty.Internals;
using LotusCore.Modules.LotusNetty.Packets;
using LotusCore.Modules.MojangLogin.MinecraftAuthModels;
using LotusCore.Modules.MojangLogin.Models;
using static LotusCore.Modules.LotusNetty.Internals.ProtocolVersionUtils;

namespace LotusCore.Interfaces
{
    public interface IServerListModule : IModuleBase
    {
        public (string ip, string port) ServerListIPRequest(string serverName);
    }
}
