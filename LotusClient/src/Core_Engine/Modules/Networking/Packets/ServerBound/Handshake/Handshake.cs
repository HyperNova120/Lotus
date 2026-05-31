using LotusCore.BaseClasses.Types;
using LotusCore.Interfaces;
using LotusCore.Modules.LotusNetty.Types;

namespace LotusCore.Modules.LotusNetty.Packets.ServerBound.Handshake
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

        ICoreModule? _coreModule;

        public override byte[] GetBytes()
        {
            /* Logging.LogDebug(
                $"ProtocolVersion Bytes:{BitConverter.ToString(VarInt_VarLong.EncodeInt(25565))}"
            ); */
            return
            [
                .. VarInt_VarLong.EncodeInt(
                    (int)_coreModule!.GetModule<INetworkModule>()!.GetProtocolVersion()
                ),
                .. StringN.GetBytes(_ServerAddress),
                .. BitConverter.GetBytes(_ServerPort),
                .. VarInt_VarLong.EncodeInt(_NextState),
            ];
        }

        public HandshakePacket(
            ICoreModule coreModule,
            string serverAddress,
            Intent nextState,
            ushort serverPort = 25565
        )
        {
            this._ServerAddress = serverAddress;
            this._ServerPort = serverPort;
            this._NextState = (int)nextState;
            _coreModule = coreModule;
        }
    }
}
