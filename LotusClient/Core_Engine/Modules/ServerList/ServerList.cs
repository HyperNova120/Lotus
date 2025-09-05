using System.Net;
using LotusCore.BaseClasses.Types;
using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.Networking;
using LotusCore.Modules.Networking.Internals;
using LotusCore.Modules.Networking.Packets;
using LotusCore.Modules.Networking.Packets.ServerBound.Handshake;
using LotusCore.Modules.Networking.Packets.ServerBound.Status;
using LotusCore.Modules.ServerList.Commands;
using LotusCore.Utils;
using LotusCore.Utils.MinecraftPaths;

namespace LotusCore.Modules.ServerList
{
    public class ServerList : IModuleBase
    {
        private NBT ServerListDat;

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand)
        {
            RegisterCommand.Invoke("list", new ListCommand(ServerListDat));
        }

        public void RegisterEvents(Action<string> RegisterEvent) { }

        public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent)
        {
            SubscribeToEvent.Invoke(
                "STATUS_Packet_Received",
                new EngineEventHandler(ProcessPacket)
            );
        }

        public ServerList()
        {
            ServerListDat = new();
            ServerListDat.ReadFromBytes(File.ReadAllBytes(MinecraftPathsStruct._ServerData));
        }

        public EngineEventResult? ProcessPacket(object? sender, IEngineEventArgs args)
        {
            PacketReceivedEventArgs eventArgs = (PacketReceivedEventArgs)args;
            var packet = eventArgs._Packet;
            switch (packet._Protocol_ID)
            {
                case 0x00:
                    HandleStatusResponse(packet);
                    break;
                case 0x01:
                    HandleStatusPingResponse(packet);
                    break;
                default:
                    Logging.LogError(
                        $"StatusHandler State 0x{packet._Protocol_ID:X} Not Implemented"
                    );
                    Core_Engine
                        .GetModule<Networking.Networking>("Networking")!
                        .DisconnectFromServer(eventArgs._RemoteHost);
                    break;
            }
            return null;
        }

        private void HandleStatusPingResponse(MinecraftServerPacket packet)
        {
            try
            {
                long value = BitConverter.ToInt64(packet._Data);
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
            (string value, int size) = StringN.DecodeBytes(packet._Data);
            Logging.LogDebug($"Response Size: {size}\n{value}");
        }

        private static void SendPingRequest(IPAddress remoteHost)
        {
            ServerConnection connection = Core_Engine
                .GetModule<Networking.Networking>("Networking")!
                .GetServerConnection(remoteHost)!;
            var connectionState = connection._ConnectionState;
            if (connectionState == Networking.Networking.ConnectionState.STATUS)
            {
                Core_Engine
                    .GetModule<Networking.Networking>("Networking")!
                    .SendPacket(connection._RemoteHost, new StatusPingRequestPacket());
            }
        }
    }
}
