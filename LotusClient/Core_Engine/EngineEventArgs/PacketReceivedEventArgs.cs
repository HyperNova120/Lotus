using System.Net;
using Core_Engine.Modules.Networking.Packets;

namespace Core_Engine.EngineEventArgs
{
    public class PacketReceivedEventArgs : EventArgs
    {
        public MinecraftServerPacket packet { get; private set; }
        public IPAddress remoteHost { get; private set; }

        public PacketReceivedEventArgs(MinecraftServerPacket packet, IPAddress remoteHost)
        {
            this.packet = packet;
            this.remoteHost = remoteHost;
        }
    }
}
