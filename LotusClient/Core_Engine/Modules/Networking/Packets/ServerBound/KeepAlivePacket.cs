using LotusCore.BaseClasses.Types;

namespace LotusCore.Modules.Networking.Packets
{
    public class KeepAlivePacket : MinecraftPacket
    {
        long _KeepAliveID;

        public KeepAlivePacket(int protocol_ID, long KeepAliveID)
        {
            this._Protocol_ID = protocol_ID;
            _KeepAliveID = KeepAliveID;
        }

        public override byte[] GetBytes()
        {
            return NetworkLong.GetBytes(_KeepAliveID);
        }
    }
}
