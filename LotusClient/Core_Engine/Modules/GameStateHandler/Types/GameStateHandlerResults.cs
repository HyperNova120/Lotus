using LotusCore.EngineEvents;
using LotusCore.Modules.GameStateHandlerModule.BaseClasses;
using LotusCore.Modules.GameStateHandlerModule.Models;
using LotusCore.Modules.MojangLogin.Models;

namespace LotusCore.Modules.GameStateHandlerModule.Types;

public class ServerCookieResult : EngineEventResult
{
    public ServerCookie? _serverCookie;

    public ServerCookieResult(ServerCookie? serverCookie)
    {
        _serverCookie = serverCookie;
    }
}

public class MojangKeyPairResult : EngineEventResult
{
    public MojangKeyPair? _mojangKeyPair;

    public MojangKeyPairResult(MojangKeyPair? mojangKeyPair)
    {
        _mojangKeyPair = mojangKeyPair;
    }
}

public class MinecraftProfileResult : EngineEventResult
{
    public MinecraftProfile? _minecraftProfile;

    public MinecraftProfileResult(MinecraftProfile? minecraftProfile)
    {
        _minecraftProfile = minecraftProfile;
    }
}
