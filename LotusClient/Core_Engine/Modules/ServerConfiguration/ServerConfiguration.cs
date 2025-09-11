using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.Networking.Packets;
using LotusCore.Modules.ServerConfig.Internals;

namespace LotusCore.Modules.ServerConfig
{
    public class ServerConfiguration : IModuleBase
    {
        private readonly ConfigurationInternals _ConfigurationInternals = new();

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

        public void RegisterEvents(Action<string> RegisterEvent) { }

        public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent)
        {
            SubscribeToEvent.Invoke(
                "CONFIG_Packet_Received",
                new EngineEventHandler(
                    (sender, args) =>
                    {
                        _ = ProcessPacket(sender, args);
                        return null;
                    }
                )
            );
            SubscribeToEvent.Invoke(
                "CONFIG_Start_Config_Process",
                new EngineEventHandler(
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
                        return null;
                    }
                )
            );
        }

        public async Task ProcessPacket(object? sender, IEngineEventArgs args)
        {
            try
            {
                PacketReceivedEventArgs eventArgs = (PacketReceivedEventArgs)args;
                MinecraftServerPacket packet = eventArgs._Packet;
                switch (packet._Protocol_ID)
                {
                    case 0x00:
                        _ConfigurationInternals.HandleCookieRequest(packet);
                        break;
                    case 0x01:
                        _ConfigurationInternals.HandlePluginMessage(packet);
                        break;
                    case 0x02:
                        _ConfigurationInternals.HandleDisconnect(packet);
                        break;
                    case 0x03:
                        _ConfigurationInternals.HandleFinishConfiguration(packet);
                        break;
                    case 0x04:
                        _ConfigurationInternals.HandleKeepAlive(packet);
                        break;
                    case 0x05:
                        _ConfigurationInternals.HandlePing(packet);
                        break;
                    case 0x06:
                        //reset chat, no use at this moment
                        break;
                    case 0x07:
                        _ConfigurationInternals.HandleRegistryData(packet);
                        break;
                    case 0x08:
                        _ConfigurationInternals.HandleRemoveResourcePack(packet);
                        break;
                    case 0x09:
                        _ConfigurationInternals.HandleAddResourcePack(packet);
                        break;
                    case 0x0A:
                        _ConfigurationInternals.HandleStoreCookie(packet);
                        break;
                    case 0x0B:
                        _ConfigurationInternals.HandleTransfer(packet);
                        break;
                    case 0x0C:
                        _ConfigurationInternals.HandleFeatureFlags(packet);
                        break;
                    case 0x0D:
                        _ConfigurationInternals.HandleUpdateTags(packet);
                        break;
                    case 0x0E:
                        _ConfigurationInternals.HandleClientboundKnownPacks(packet);
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
