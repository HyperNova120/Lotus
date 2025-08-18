using System.Text;
using Core_Engine.BaseClasses.Types;
using Core_Engine.Modules.GameStateHandler.BaseClasses;

namespace Core_Engine.Modules.Networking.Packets.ServerBound.Configuration
{
    public class ServerboundKnownPacksPacket : MinecraftPacket
    {
        public List<RegistryData> _KnownPacks = new();

        public ServerboundKnownPacksPacket()
        {
            _Protocol_ID = 0x07;
        }

        public override byte[] GetBytes()
        {
            List<byte> tmp = new();

            throw new NotImplementedException();
        }
    }
}
