using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.Networking.Internals;
using LotusCore.Modules.Networking.Packets;
using LotusCore.Modules.Networking.Types;
using LotusCore.Modules.ServerPlay.Internals;

namespace LotusCore.Modules.ServerPlay;

public class ServerPlayHandler : IModuleBase
{
    private readonly ServerPlayInternals _playInternals = new();

    public ServerPlayHandler() { }

    public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

    public void RegisterEvents(Action<string> RegisterEvent) { }

    public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent)
    {
        SubscribeToEvent.Invoke(
            "PLAY_Packet_Received",
            new EngineEventHandler(
                (sender, args) =>
                {
                    _ = ProcessPacket(sender, args);
                    return null;
                }
            )
        );

        SubscribeToEvent.Invoke(
            "CONFIG_Complete",
            new EngineEventHandler(
                (sender, args) =>
                {
                    _playInternals.ServerboundPlayerSession(
                        ((ConnectionEventArgs)args)._remoteHostID
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
            MinecraftServerPacket packet = eventArgs._packet;
            ServerConnection serverConnection = Core_Engine
                .InvokeEvent<ServerConnectionResult>(
                    "NETWORKING_GetServerConnection",
                    new GuidEngineArgs(packet._remoteHostID)
                )!
                ._serverConnection!;
            if (packet._protocol_ID == 0x00)
            {
                HandleBundleDelimiter(packet);
                return;
            }
            else if (serverConnection._packetInfo._activeBundleDelimiter)
            {
                serverConnection._packetInfo._bundledPackets.Enqueue(packet);
                return;
            }
            HandlePacketSwitch(packet);
        }
        catch (Exception e)
        {
            Logging.LogError($"ServerPlayHandler; ProcessPacket ERROR: {e}");
            //Core_Engine.SignalInteractiveFree(Core_Engine.State.Play);
        }
    }

    private void HandlePacketSwitch(MinecraftServerPacket packet)
    {
        try
        {
            switch (packet._protocol_ID)
            {
                case 0x3A:
                    Logging.LogDebug("HandlePlayerChatMessage");
                    _playInternals.HandlePlayerChatMessage(packet);
                    break;
                case 0x72:
                    Logging.LogDebug("HandleSystemChatMessage");
                    _playInternals.HandleSystemChatMessage(packet);
                    break;
                default:
                    //Logging.LogError($"Play Packet ID: 0x{packet._Protocol_ID:X} not implemented");
                    break;
            }
        }
        catch (Exception e)
        {
            Logging.LogError($"Play HandlePacketSwitch: {e.ToString()}");
        }
    }

    private void HandleBundleDelimiter(MinecraftServerPacket packet)
    {
        ServerConnection serverConnection = Core_Engine
            .InvokeEvent<ServerConnectionResult>(
                "NETWORKING_GetServerConnection",
                new GuidEngineArgs(packet._remoteHostID)
            )!
            ._serverConnection!;
        if (!serverConnection._packetInfo._activeBundleDelimiter)
        {
            serverConnection._packetInfo._activeBundleDelimiter = true;
        }
        else
        {
            serverConnection._packetInfo._activeBundleDelimiter = false;
            while (serverConnection._packetInfo._bundledPackets.Count != 0)
            {
                var packetToProcess = serverConnection._packetInfo._bundledPackets.Dequeue();
                HandlePacketSwitch(packetToProcess);
            }
        }
    }
}
