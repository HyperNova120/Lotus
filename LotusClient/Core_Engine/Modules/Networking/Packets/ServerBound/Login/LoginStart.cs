using LotusCore.Modules.LotusNetty.Packets;
using LotusCore.BaseClasses.Types;

namespace LotusCore.Modules.LotusNetty.Packets.ServerBound.Login
{
    public class LoginStartPacket : MinecraftPacket
    {
        public string _Username { get; set; }
        public Guid _uuid { get; set; }

        public LoginStartPacket(string username, Guid uuid)
        {
            _protocol_ID = 0x00;
            this._Username = username;
            this._uuid = uuid;
        }

        public override byte[] GetBytes()
        {
            return [.. StringN.GetBytes(_Username), .. _uuid.ToByteArray()];
        }
    }
}
