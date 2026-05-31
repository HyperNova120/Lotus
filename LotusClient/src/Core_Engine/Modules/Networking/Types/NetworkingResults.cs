using LotusCore.EngineEvents;
using LotusCore.Modules.LotusNetty.Internals;

namespace LotusCore.Modules.LotusNetty.Types;

public class ProtocolVersionResult : EngineEventResult
{
    public ProtocolVersionUtils.ProtocolVersion _version;

    public ProtocolVersionResult(ProtocolVersionUtils.ProtocolVersion version)
    {
        _version = version;
    }
}

public class ServerConnectionResult : EngineEventResult
{
    public ServerConnection? _serverConnection;

    public ServerConnectionResult(ServerConnection? serverConnection)
    {
        _serverConnection = serverConnection;
    }
}
