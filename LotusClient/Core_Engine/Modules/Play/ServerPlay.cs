using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.LotusNetty.Internals;
using LotusCore.Modules.LotusNetty.Packets;
using LotusCore.Modules.LotusNetty.Types;
using LotusCore.Modules.ServerPlay.Internals;

namespace LotusCore.Modules.ServerPlay;

public class ServerPlayHandler : IServerPlayHandlerModule, IModuleBase, IPacketHandler
{
    private ServerPlayInternals _playInternals;

    private INetworkModule _networkingModule;

    private IServerChatModule _serverChat;

    public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

    public void RegisterEvents(Action<string> RegisterEvent) { }

    public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent) { }

    public void LinkModules()
    {
        _networkingModule = Core_Engine.GetModule<INetworkModule>("Networking")!;
        _serverChat = Core_Engine.GetModule<IServerChatModule>("ServerChat")!;
        _playInternals = new(_serverChat, _networkingModule);
    }

    public void InitPlaySession(Guid remoteHostID)
    {
        _serverChat.StartChatSession(remoteHostID);
    }

    public async Task ProcessPacket(MinecraftServerPacket packet)
    {
        try
        {
            ServerConnection serverConnection = _networkingModule.GetServerConnection(
                packet._remoteHostID
            )!;
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
                case 0x2B:
                    //keep alive
                    Logging.LogDebug("PLAY Keep Alive");
                    _playInternals.HandleKeepAlive(packet);
                    break;
                case 0x3F:
                    Logging.LogDebug("HandlePlayerChatMessage");
                    _playInternals.HandlePlayerChatMessage(packet);
                    break;
                case 0x77:
                    Logging.LogDebug("HandleSystemChatMessage");
                    _playInternals.HandleSystemChatMessage(packet);
                    break;
                default:
                    //Logging.LogError($"Play Packet ID: 0x{packet._protocol_ID:X} not implemented");
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
        ServerConnection serverConnection = _networkingModule.GetServerConnection(
            packet._remoteHostID
        )!;

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
