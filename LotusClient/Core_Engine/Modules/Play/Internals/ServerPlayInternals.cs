using Core_Engine.Interfaces;
using Core_Engine.Modules.Networking.Internals;
using Core_Engine.Modules.Networking.Packets;

namespace Core_Engine.Modules.ServerPlay.Internals;

public class ServerPlayInternals
{
    private IGameStateHandler _GameStateHandler;
    private Networking.Networking _NetworkingManager;

    public ServerPlayInternals()
    {
        _GameStateHandler = Core_Engine.GetModule<IGameStateHandler>("GameStateHandler")!;
        _NetworkingManager = Core_Engine.GetModule<Networking.Networking>("Networking")!;
    }

    public void HandleBundleDelimiter(MinecraftServerPacket packet)
    {
        ServerConnection serverConnection = _NetworkingManager.GetServerConnection(
            packet._RemoteHost
        )!;
        if (!serverConnection._ActiveBundleDelimiter)
        {
            serverConnection._ActiveBundleDelimiter = true;
        }
    }
}
