using Core_Engine.EngineEventArgs;
using Core_Engine.Interfaces;
using Core_Engine.Modules.Networking;
using Core_Engine.Modules.Networking.Packets;
using Core_Engine.Modules.Networking.Pakcets.ServerBound.Handshake;
using Core_Engine.Modules.Networking.Pakcets.ServerBound.Status;
using Core_Engine.Modules.Networking.Types;

namespace Core_Engine.Modules.ServerStatus
{
    public class ServerStatus : IModuleBase
    {
        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

        public void RegisterEvents(Action<string> RegisterEvent) { }

        public void SubscribeToEvents(Action<string, EventHandler> SubscribeToEvent)
        {
            SubscribeToEvent.Invoke("STATUS_Packet_Received", new EventHandler(ProcessPacket));
        }

        public void ProcessPacket(object? sender, EventArgs args)
        {
            PacketReceivedEventArgs eventArgs = (PacketReceivedEventArgs)args;
            var packet = eventArgs.packet;
            switch (packet.protocol_id)
            {
                case 0x00:
                    HandleStatusResponse(packet);
                    break;
                case 0x01:
                    HandleStatusPingResponse(packet);
                    break;
                default:
                    Logging.LogError(
                        $"StatusHandler State 0x{packet.protocol_id:X} Not Implemented"
                    );
                    break;
            }
        }

        private void HandleStatusPingResponse(MinecraftServerPacket packet)
        {
            try
            {
                long value = BitConverter.ToInt64(packet.data);
                Logging.LogDebug(
                    $"Response: {value} Ping:{(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - value}m"
                );
            }
            catch (Exception e)
            {
                Logging.LogDebug(e.ToString());
            }
        }

        private void HandleStatusResponse(MinecraftServerPacket packet)
        {
            (string value, int size) = StringN.DecodeBytes(packet.data);
            Logging.LogDebug($"Response Size: {size}\n{value}");
        }

        private static void SendPingRequest()
        {
            var connectionState = Core_Engine
                .GetModule<Networking.Networking>("Networking")!
                .connectionState;
            if (connectionState == Networking.Networking.ConnectionState.STATUS)
            {
                Core_Engine
                    .GetModule<Networking.Networking>("Networking")!
                    .SendPacket(new StatusPingRequestPacket());
            }
        }
    }
}
