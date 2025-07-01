using Core_Engine.BaseClasses.Types;

namespace Core_Engine.Modules.Networking.Packets.ServerBound.Configuration
{
    public class ClientInformationPacket : MinecraftPacket
    {
        public string locale = "";
        public byte viewDistance;
        public int chatMode;
        public bool chatColors;
        public byte displayedSkinParts;
        public int mainHand;
        public bool enableTextFiltering;
        public bool allowServerListings;
        public int particleStatus;

        public enum displayedSkinPartsFlags
        {
            CapeEnabled = 0x01,
            JacketEnabled = 0x02,
            LeftSleeveEnabled = 0x04,
            RigthSleeveEnabled = 0x08,
            LeftPantsLegEnabled = 0x10,
            RightPantsLegEnabled = 0x20,
            HatEnabled = 0x40,
        }

        public ClientInformationPacket()
        {
            this.protocol_id = 0x00;
        }

        public override byte[] GetBytes()
        {
            return
            [
                .. StringN.GetBytes(locale),
                viewDistance,
                .. VarInt_VarLong.EncodeInt(chatMode),
                (byte)((chatColors) ? 0x01 : 0x00),
                displayedSkinParts,
                .. VarInt_VarLong.EncodeInt(mainHand),
                (byte)((enableTextFiltering) ? 0x01 : 0x00),
                (byte)((allowServerListings) ? 0x01 : 0x00),
                .. VarInt_VarLong.EncodeInt(particleStatus),
            ];
        }
    }
}
