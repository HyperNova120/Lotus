using Core_Engine.Modules.Networking.Packets;

namespace Core_Engine.EngineEventArgs
{
    public class PacketReceivedEventArgs : EventArgs
    {
        public MinecraftServerPacket packet { get; private set; }

        public PacketReceivedEventArgs(MinecraftServerPacket packet)
        {
            this.packet = packet;
        }
    }
}
