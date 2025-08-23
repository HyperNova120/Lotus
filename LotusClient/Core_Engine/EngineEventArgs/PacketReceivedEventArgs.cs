using System.Net;
using LotusCore.Modules.Networking.Packets;

namespace LotusCore.EngineEventArgs
{
    public class PacketReceivedEventArgs : EventArgs
    {
        public MinecraftServerPacket _Packet { get; private set; }
        public IPAddress _RemoteHost { get; private set; }

        public PacketReceivedEventArgs(MinecraftServerPacket packet, IPAddress remoteHost)
        {
            this._Packet = packet;
            this._RemoteHost = remoteHost;
        }
    }
}
