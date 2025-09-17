using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.Networking.Internals;
using LotusCore.Modules.Networking.Packets;
using LotusCore.Modules.ServerPlay.Internals;

namespace LotusCore.Modules.ServerPlay;

public class ServerPlayHandler : IModuleBase
{
    private Networking.Networking _NetworkingManager;
    private readonly ServerPlayInternals _playInternals = new();

    public ServerPlayHandler()
    {
        _NetworkingManager = Core_Engine.GetModule<Networking.Networking>("Networking")!;
    }

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
                        ((ConnectionEventArgs)args)._RemoteHost
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
            ServerConnection serverConnection = _NetworkingManager.GetServerConnection(
                packet._RemoteHost
            )!;
            if (packet._Protocol_ID == 0x00)
            {
                HandleBundleDelimiter(packet);
                return;
            }
            else if (serverConnection._ActiveBundleDelimiter)
            {
                serverConnection._BundledPackets.Enqueue(packet);
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
            switch (packet._Protocol_ID)
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
        ServerConnection serverConnection = _NetworkingManager.GetServerConnection(
            packet._RemoteHost
        )!;
        if (!serverConnection._ActiveBundleDelimiter)
        {
            serverConnection._ActiveBundleDelimiter = true;
        }
        else
        {
            serverConnection._ActiveBundleDelimiter = false;
            while (serverConnection._BundledPackets.Count != 0)
            {
                var packetToProcess = serverConnection._BundledPackets.Dequeue();
                HandlePacketSwitch(packetToProcess);
            }
        }
    }
}
