using Core_Engine.EngineEventArgs;
using Core_Engine.Interfaces;
using Core_Engine.Modules.Networking.Packets;
using Core_Engine.Modules.ServerConfig.Internals;

namespace Core_Engine.Modules.ServerConfig
{
    public class ServerConfiguration : IModuleBase
    {
        private readonly ConfigurationInternals configurationInternals = new();

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

        public void RegisterEvents(Action<string> RegisterEvent) { }

        public void SubscribeToEvents(Action<string, EventHandler> SubscribeToEvent)
        {
            SubscribeToEvent.Invoke(
                "CONFIG_Packet_Received",
                new EventHandler(
                    (sender, args) =>
                    {
                        _ = ProcessPacket(sender, args);
                    }
                )
            );
        }

        public async Task ProcessPacket(object? sender, EventArgs args)
        {
            PacketReceivedEventArgs eventArgs = (PacketReceivedEventArgs)args;
            MinecraftServerPacket packet = eventArgs.packet;
            switch (packet.protocol_id)
            {
                case 0x03:
                    configurationInternals.FinishConfiguration(packet);
                break;
                case 0x0A:
                    configurationInternals.StoreCookie(packet);
                    break;
                case 0x0B:
                    configurationInternals.Transfer(packet);
                    break;
                case 0x07:
                    configurationInternals.RegistryData(packet);
                    break;
                default:
                    Logging.LogError(
                        $"ServerConfiguration State 0x{packet.protocol_id:X} Not Implemented"
                    );
                    /* Core_Engine
                        .GetModule<Networking.Networking>("Networking")!
                        .DisconnectFromServer(packetArgs.remoteHost);
                    Core_Engine.SignalInteractiveResetServerHolds(); */

                    break;
            }
        }
    }
}
