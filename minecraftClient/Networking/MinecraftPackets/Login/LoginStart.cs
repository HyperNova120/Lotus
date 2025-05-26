using MinecraftNetworking;
using MinecraftNetworking.Types;

namespace MinecraftNetworking.Packets
{
    public class LoginStartPacket : MinecraftPacket
    {
        public string username { get; set; }
        public Guid uuid { get; set; }

        public LoginStartPacket(string username, Guid uuid)
        {
            protocol_id = 0x00;
            this.username = username;
            this.uuid = uuid;
        }

        public override byte[] GetBytes()
        {
            return [.. StringN.GetBytes(username), .. uuid.ToByteArray()];
        }
    }
}
