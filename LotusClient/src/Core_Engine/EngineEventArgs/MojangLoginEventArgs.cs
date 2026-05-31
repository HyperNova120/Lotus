using LotusCore.Modules.MojangLogin.MinecraftAuthModels;
using LotusCore.Modules.MojangLogin.Models;

namespace LotusCore.EngineEventArgs;

public class MojangLoginEventArgs : IEngineEventArgs
{
    public MinecraftAuthResponseModel _AuthModel;

    public MinecraftProfile _UserProfile;
}
