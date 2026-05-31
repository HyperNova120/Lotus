using LotusCore.BaseClasses;
using LotusCore.EngineEventArgs;
using LotusCore.Modules.GameStateHandlerModule.BaseClasses;

namespace LotusCore.Modules.GameStateHandlerModule.Types;

public class ServerCookieArg : IEngineEventArgs
{
    public ServerCookie _serverCookie;

    public ServerCookieArg(ServerCookie serverCookie)
    {
        _serverCookie = serverCookie;
    }
}

public class UpdateServerRegistryDataArgs : IEngineEventArgs
{
    public RegistryData _registryData;
    public bool _overwrite;
    public bool _replace;

    public UpdateServerRegistryDataArgs(
        RegistryData registryData,
        bool overwrite = true,
        bool replace = false
    )
    {
        _registryData = registryData;
        _overwrite = overwrite;
        _replace = replace;
    }
}

public class ServerResourcePackArg : IEngineEventArgs
{
    public ResourcePack _resourcePack;

    public ServerResourcePackArg(ResourcePack resourcePack)
    {
        _resourcePack = resourcePack;
    }
}

public class AddServerTagArgs : IEngineEventArgs
{
    public Identifier _registry;
    public Identifier _tagName;
    public List<int> _entries;

    public AddServerTagArgs(Identifier registry, Identifier tagName, List<int> entries)
    {
        _registry = registry;
        _tagName = tagName;
        _entries = entries;
    }
}
