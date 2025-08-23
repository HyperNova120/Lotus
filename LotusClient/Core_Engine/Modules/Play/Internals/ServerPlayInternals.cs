using LotusCore.Interfaces;
using LotusCore.Modules.Networking.Internals;
using LotusCore.Modules.Networking.Packets;

namespace LotusCore.Modules.ServerPlay.Internals;

public class ServerPlayInternals
{
    private IGameStateHandler _GameStateHandler;
    private Networking.Networking _NetworkingManager;

    public ServerPlayInternals()
    {
        _GameStateHandler = Core_Engine.GetModule<IGameStateHandler>("GameStateHandler")!;
        _NetworkingManager = Core_Engine.GetModule<Networking.Networking>("Networking")!;
    }

}
