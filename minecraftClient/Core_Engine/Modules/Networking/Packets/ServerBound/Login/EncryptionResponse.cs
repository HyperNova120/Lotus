using Core_Engine.Modules.Networking.Packets;
using Core_Engine.Modules.Networking.Types;

namespace Core_Engine.Modules.Networking.Pakcets.ServerBound.Login
{
    public class EncryptionResponsePacket : MinecraftPacket
    {
        public byte[] SharedSecret { get; set; }
        public byte[] VerifyToken { get; set; }

        public override byte[] GetBytes()
        {
            return
            [
                .. PrefixedArray.GetBytes(SharedSecret),
                .. PrefixedArray.GetBytes(VerifyToken),
            ];
        }

        public EncryptionResponsePacket(byte[] sharedSecret, byte[] verifyToken)
        {
            protocol_id = 0x01;
            SharedSecret = sharedSecret;
            VerifyToken = verifyToken;
        }
    }
}
