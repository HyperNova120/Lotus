using LotusCore.BaseClasses.Types;

namespace LotusCore.Modules.Networking.Packets
{
    public class PongPacket : MinecraftPacket
    {
        int _PongID;

        public PongPacket(int protocol_ID, int PongID)
        {
            this._Protocol_ID = protocol_ID;
            _PongID = PongID;
        }

        public override byte[] GetBytes()
        {
            return NetworkInt.GetBytes(_PongID);
        }
    }
}
