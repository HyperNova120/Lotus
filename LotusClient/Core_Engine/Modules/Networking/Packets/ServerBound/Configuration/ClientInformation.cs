using LotusCore.BaseClasses.Types;

namespace LotusCore.Modules.Networking.Packets.ServerBound.Configuration
{
    public class ClientInformationPacket : MinecraftPacket
    {
        public string _Locale = "";
        public byte _ViewDistance;
        public int _ChatMode;
        public bool _ChatColors;
        public byte _DisplayedSkinParts;
        public int _MainHand;
        public bool _EnableTextFiltering;
        public bool _AllowServerListings;
        public int _ParticleStatus;

        public ClientInformationPacket()
        {
            this._Protocol_ID = 0x00;
        }

        public override byte[] GetBytes()
        {
            return
            [
                .. StringN.GetBytes(_Locale),
                _ViewDistance,
                .. VarInt_VarLong.EncodeInt(_ChatMode),
                (byte)((_ChatColors) ? 0x01 : 0x00),
                _DisplayedSkinParts,
                .. VarInt_VarLong.EncodeInt(_MainHand),
                (byte)((_EnableTextFiltering) ? 0x01 : 0x00),
                (byte)((_AllowServerListings) ? 0x01 : 0x00),
                .. VarInt_VarLong.EncodeInt(_ParticleStatus),
            ];
        }
    }
}
