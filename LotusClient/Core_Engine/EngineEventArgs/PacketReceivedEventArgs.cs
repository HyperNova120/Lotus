using System.Net;
using LotusCore.Modules.Networking.Packets;

namespace LotusCore.EngineEventArgs
{
    public class PacketReceivedEventArgs : IEngineEventArgs
    {
        public MinecraftServerPacket _packet { get; private set; }
        public Guid _remoteHostID { get; private set; }

        public PacketReceivedEventArgs(MinecraftServerPacket packet, Guid remoteHostID)
        {
            this._packet = packet;
            this._remoteHostID = remoteHostID;
        }
    }
}
