using System.Net;
using LotusCore.EngineEvents;

namespace LotusCore.EngineEvents;

public class ServerListServerIPResult : EngineEventResult
{
    public string _ip = "";
    public string _port = "";
}
