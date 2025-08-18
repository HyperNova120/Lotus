using Core_Engine.BaseClasses;
using Core_Engine.BaseClasses.Types;

namespace Core_Engine.Modules.Networking.Packets.ServerBound.Configuration
{
    public class CookieResponsepacket : MinecraftPacket
    {
        public Identifier? _Key;
        public byte[] _Payload = [];

        public CookieResponsepacket()
        {
            _Protocol_ID = 0x01;
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
