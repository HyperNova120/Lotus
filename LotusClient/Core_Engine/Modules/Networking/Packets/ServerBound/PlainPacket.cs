using LotusCore.BaseClasses.Types;
using Microsoft.Identity.Client;

namespace LotusCore.Modules.LotusNetty.Packets
{
    public class PlainPacket : MinecraftPacket
    {
        public byte[] _data;

        public PlainPacket(int protocol_ID, byte[] data)
        {
            this._protocol_ID = protocol_ID;
            _data = data;
        }

        public override byte[] GetBytes()
        {
            return _data;
        }
    }
}
