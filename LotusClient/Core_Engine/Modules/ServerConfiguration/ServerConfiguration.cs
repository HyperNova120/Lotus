using Core_Engine.EngineEventArgs;
using Core_Engine.Interfaces;

namespace Core_Engine.Modules.ServerConfig
{
    public class ServerConfiguration : IModuleBase
    {
        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

        public void RegisterEvents(Action<string> RegisterEvent) { }

        public void SubscribeToEvents(Action<string, EventHandler> SubscribeToEvent)
        {
            SubscribeToEvent.Invoke("CONFIG_Packet_Received", new EventHandler(ProcessPacket));
        }

        public void ProcessPacket(object? sender, EventArgs args)
        {
            PacketReceivedEventArgs packetArgs = (PacketReceivedEventArgs)args;
            switch (packetArgs.packet.protocol_id)
            {
                case 0x00:
                    break;
                default:
                    Logging.LogError(
                        $"ServerConfiguration State 0x{packetArgs.packet.protocol_id:X} Not Implemented"
                    );
                    Core_Engine
                        .GetModule<Networking.Networking>("Networking")!
                        .DisconnectFromServer(packetArgs.remoteHost);

                    break;
            }
        }
    }
}
