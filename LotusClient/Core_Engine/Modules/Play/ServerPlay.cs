using LotusCore.EngineEventArgs;
using LotusCore.Interfaces;
using LotusCore.Modules.Networking.Internals;
using LotusCore.Modules.Networking.Packets;
using LotusCore.Modules.ServerPlay.Internals;

namespace LotusCore.Modules.ServerPlay;

public class ServerPlayHandler : IModuleBase
{
    private Networking.Networking _NetworkingManager;
    private readonly ServerPlayInternals playInternals = new();

    public ServerPlayHandler()
    {
        _NetworkingManager = Core_Engine.GetModule<Networking.Networking>("Networking")!;
    }

    public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

    public void RegisterEvents(Action<string> RegisterEvent) { }

    public void SubscribeToEvents(Action<string, EventHandler> SubscribeToEvent)
    {
        SubscribeToEvent.Invoke(
            "PLAY_Packet_Received",
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
            Core_Engine.SignalInteractiveFree(Core_Engine.State.Play);
        }
    }

    private void HandlePacketSwitch(MinecraftServerPacket packet)
    {
        switch (packet._Protocol_ID) { }
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
