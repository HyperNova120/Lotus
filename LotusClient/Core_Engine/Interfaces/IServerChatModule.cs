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
    public interface IServerChatModule : IModuleBase
    {
        public void StartChatSession(Guid remoteHostID);

        public void ReceivePlayerChatMessagePacket(MinecraftServerPacket packet, Guid remoteHostID);
    }
}
