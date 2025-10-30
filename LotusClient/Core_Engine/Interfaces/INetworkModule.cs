using System.Net;
using LotusCore.EngineEvents;
using LotusCore.Modules.LotusNetty;
using LotusCore.Modules.LotusNetty.Internals;
using LotusCore.Modules.LotusNetty.Packets;
using static LotusCore.Modules.LotusNetty.Internals.ProtocolVersionUtils;

namespace LotusCore.Interfaces
{
    public interface INetworkModule
    {
        public void LoginSuccessful(Guid remoteHostID);

        public ProtocolVersion GetProtocolVersion();

        public int SendPacket(
            Guid RemoteHostID,
            MinecraftPacket packet,
            bool HoldPacketInBuffer = false
        );

        public int SendPackets(
            Guid RemoteHostId,
            IEnumerable<MinecraftPacket> packets,
            bool HoldPacketInBuffer = false
        );

        public int SendBufferedPackets(Guid RemoteHostId);

        public Guid? ConnectToServer(string ip, int port = 25565);

        public void DisconnectFromServer(Guid remoteHostID);

        public ServerConnection? GetServerConnection(Guid connectionID);

        public Guid? GetServerConnectionInState(
            IPAddress connectionID,
            IEnumerable<ConnectionState> connectionStates
        );

        public bool IsClientConnectedToPrimaryServer();

        public void SetIsClientConnectedToPrimaryServer(bool value);
    }
}
