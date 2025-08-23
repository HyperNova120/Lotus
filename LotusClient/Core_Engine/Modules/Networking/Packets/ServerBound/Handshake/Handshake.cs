using LotusCore.BaseClasses.Types;

namespace LotusCore.Modules.Networking.Packets.ServerBound.Handshake
{
    public class HandshakePacket : MinecraftPacket
    {
        public enum Intent
        {
            Status = 1,
            Login = 2,
            Transfer = 3,
        }

        public PacketBoundTo _BoundTo = PacketBoundTo.Server;

        public string _ServerAddress;
        public ushort _ServerPort = 25565;
        public int _NextState = 1;

        public override byte[] GetBytes()
        {
            /* Logging.LogDebug(
                $"ProtocolVersion Bytes:{BitConverter.ToString(VarInt_VarLong.EncodeInt(25565))}"
            ); */
            return
            [
                .. VarInt_VarLong.EncodeInt(
                    (int)Core_Engine.GetModule<Networking>("Networking")!._ProtocolVersion
                ),
                .. StringN.GetBytes(_ServerAddress),
                .. BitConverter.GetBytes(_ServerPort),
                .. VarInt_VarLong.EncodeInt(_NextState),
            ];
        }

        public HandshakePacket(string serverAddress, Intent nextState, ushort serverPort = 25565)
        {
            this._ServerAddress = serverAddress;
            this._ServerPort = serverPort;
            this._NextState = (int)nextState;
        }
    }
}
