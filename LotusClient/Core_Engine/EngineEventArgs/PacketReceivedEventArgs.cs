using System.Net;
using Core_Engine.Modules.Networking.Packets;

namespace Core_Engine.EngineEventArgs
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
