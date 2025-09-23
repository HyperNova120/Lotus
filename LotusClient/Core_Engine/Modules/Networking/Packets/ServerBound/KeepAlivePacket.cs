using LotusCore.BaseClasses.Types;
using Microsoft.Identity.Client;

namespace LotusCore.Modules.Networking.Packets
{
    public class KeepAlivePacket : MinecraftPacket
    {
        public long _keepAliveID;

        public KeepAlivePacket(int protocol_ID, long KeepAliveID)
        {
            this._protocol_ID = protocol_ID;
            _keepAliveID = KeepAliveID;
        }

        public override byte[] GetBytes()
        {
            return NetworkLong.GetBytes(_keepAliveID);
        }
    }
}
