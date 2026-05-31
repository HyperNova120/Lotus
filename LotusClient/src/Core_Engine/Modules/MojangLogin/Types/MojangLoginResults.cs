using LotusCore.EngineEvents;
using LotusCore.Modules.MojangLogin.MinecraftAuthModels;
using LotusCore.Modules.MojangLogin.Models;

namespace LotusCore.Modules.MojangLogin.Types;

public class UserProfileResult : EngineEventResult
{
    public MinecraftProfile? _userProfile;

    public UserProfileResult(MinecraftProfile? userProfile)
    {
        _userProfile = userProfile;
    }
}

public class MinecraftAuthResult : EngineEventResult
{
    public MinecraftAuthResponseModel? _minecraftAuth;

    public MinecraftAuthResult(MinecraftAuthResponseModel? minecraftAuth)
    {
        _minecraftAuth = minecraftAuth;
    }
}
