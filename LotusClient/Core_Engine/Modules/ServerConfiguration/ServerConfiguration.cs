using Core_Engine.EngineEventArgs;
using Core_Engine.Interfaces;
using Core_Engine.Modules.Networking.Packets;
using Core_Engine.Modules.ServerConfig.Internals;

namespace Core_Engine.Modules.ServerConfig
{
    public class ServerConfiguration : IModuleBase
    {
        private readonly ConfigurationInternals _ConfigurationInternals = new();

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
            SubscribeToEvent.Invoke(
                "CONFIG_Start_Config_Process",
                new EventHandler(
                    (sender, args) =>
                    {
                        ConnectionEventArgs connectionEventArgs = (ConnectionEventArgs)args;
                        /* configurationInternals.SendServerboundPluginMessage(
                            connectionEventArgs.remoteHost
                        );
                        configurationInternals.SendServerboundClientInformation(
                            connectionEventArgs.remoteHost
                        ); */
                        _ConfigurationInternals.SendConfigBrandAndClientInfo(
                            connectionEventArgs._RemoteHost
                        );
                    }
                )
            );
        }

        public async Task ProcessPacket(object? sender, EventArgs args)
        {
            try
            {
                PacketReceivedEventArgs eventArgs = (PacketReceivedEventArgs)args;
                MinecraftServerPacket packet = eventArgs._Packet;
                switch (packet._Protocol_ID)
                {
                    case 0x07:
                        _ConfigurationInternals.RegistryData(packet);
                        break;
                    case 0x0A:
                        _ConfigurationInternals.StoreCookie(packet);
                        break;
                    case 0x0B:
                        _ConfigurationInternals.Transfer(packet);
                        break;
                    case 0x0E:
                        _ConfigurationInternals.ClientboundKnownPacks(packet);
                        break;
                    default:
                        Logging.LogError(
                            $"ServerConfiguration State 0x{packet._Protocol_ID:X} Not Implemented"
                        );
                        /* Core_Engine
                            .GetModule<Networking.Networking>("Networking")!
                            .DisconnectFromServer(packetArgs.remoteHost);
                        Core_Engine.SignalInteractiveResetServerHolds(); */

                        break;
                }
            }
            catch (Exception e)
            {
                Logging.LogError($"ServerConfiguration; ProcessPacket ERROR: {e}");
                /* if (Core_Engine.CurrentState == Core_Engine.State.Waiting)
                {
                    Core_Engine.CurrentState = Core_Engine.State.Interactive;
                } */
            }
        }
    }
}
