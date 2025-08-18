using Core_Engine.Modules.Networking.Packets;
using Core_Engine.BaseClasses.Types;

namespace Core_Engine.Modules.Networking.Packets.ServerBound.Login
{
    public class LoginStartPacket : MinecraftPacket
    {
        public string _Username { get; set; }
        public Guid _uuid { get; set; }

        public LoginStartPacket(string username, Guid uuid)
        {
            _Protocol_ID = 0x00;
            this._Username = username;
            this._uuid = uuid;
        }

        public override byte[] GetBytes()
        {
            return [.. StringN.GetBytes(_Username), .. _uuid.ToByteArray()];
        }
    }
}
