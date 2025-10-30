using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;

namespace LotusCore.Modules.LotusNetty.Packets.ServerBound.Configuration
{
    public class CookieResponsepacket : MinecraftPacket
    {
        public Identifier? _Key;
        public byte[] _Payload = [];

        public CookieResponsepacket()
        {
            _protocol_ID = 0x01;
        }

        public override byte[] GetBytes()
        {
            return
            [
                .. StringN.GetBytes(_Key?.IdentifierString ?? ""),
                .. PrefixedOptional.GetBytes(PrefixedArray.GetBytes(_Payload)),
            ];
        }
    }
}
