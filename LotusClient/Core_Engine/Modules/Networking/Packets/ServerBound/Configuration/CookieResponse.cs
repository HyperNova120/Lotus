using Core_Engine.BaseClasses.Types;

namespace Core_Engine.Modules.Networking.Packets.ServerBound.Configuration
{
    public class CookieResponsepacket : MinecraftPacket
    {
        public string Key = "";
        public byte[] Payload = [];

        public CookieResponsepacket()
        {
            protocol_id = 0x01;
        }

        public override byte[] GetBytes()
        {
            return
            [
                .. StringN.GetBytes(Key),
                .. PrefixedOptional.GetBytes(PrefixedArray.GetBytes(Payload)),
            ];
        }
    }
}
