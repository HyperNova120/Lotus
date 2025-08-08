using Core_Engine.BaseClasses.Types;

namespace Core_Engine.Modules.Networking.Packets.ServerBound.Handshake
{
    public class HandshakePacket : MinecraftPacket
    {
        public enum Intent
        {
            Status = 1,
            Login = 2,
            Transfer = 3,
        }

        public PacketBoundTo BoundTo = PacketBoundTo.Server;

        public string serverAddress;
        public ushort serverPort = 25565;
        public int nextState = 1;

        public override byte[] GetBytes()
        {
            /* Logging.LogDebug(
                $"ProtocolVersion Bytes:{BitConverter.ToString(VarInt_VarLong.EncodeInt(25565))}"
            ); */
            return
            [
                .. VarInt_VarLong.EncodeInt(Networking.protocolVersion),
                .. StringN.GetBytes(serverAddress),
                .. BitConverter.GetBytes(serverPort),
                .. VarInt_VarLong.EncodeInt(nextState),
            ];
        }

        public HandshakePacket(string serverAddress, Intent nextState, ushort serverPort = 25565)
        {
            this.serverAddress = serverAddress;
            this.serverPort = serverPort;
            this.nextState = (int)nextState;
        }
    }
}
