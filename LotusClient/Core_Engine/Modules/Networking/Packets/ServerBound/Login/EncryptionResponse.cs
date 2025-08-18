using Core_Engine.Modules.Networking.Packets;
using Core_Engine.BaseClasses.Types;

namespace Core_Engine.Modules.Networking.Packets.ServerBound.Login
{
    public class EncryptionResponsePacket : MinecraftPacket
    {
        public byte[] _SharedSecret { get; set; }
        public byte[] _VerifyToken { get; set; }

        public override byte[] GetBytes()
        {
            return
            [
                .. PrefixedArray.GetBytes(_SharedSecret),
                .. PrefixedArray.GetBytes(_VerifyToken),
            ];
        }

        public EncryptionResponsePacket(byte[] sharedSecret, byte[] verifyToken)
        {
            _Protocol_ID = 0x01;
            _SharedSecret = sharedSecret;
            _VerifyToken = verifyToken;
        }
    }
}
