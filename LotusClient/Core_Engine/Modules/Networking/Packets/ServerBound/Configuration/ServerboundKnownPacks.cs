using System.Text;
using Core_Engine.BaseClasses.Types;
using Core_Engine.Modules.GameStateHandler.BaseClasses;

namespace Core_Engine.Modules.Networking.Packets.ServerBound.Configuration
{
    public class ServerboundKnownPacksPacket : MinecraftPacket
    {
        public List<RegistryData> KnownPacks = new();

        public ServerboundKnownPacksPacket()
        {
            protocol_id = 0x07;
        }

        public override byte[] GetBytes()
        {
            throw new NotImplementedException();
        }
    }
}
