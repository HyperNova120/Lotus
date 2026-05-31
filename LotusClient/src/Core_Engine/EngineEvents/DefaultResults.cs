using System.Net;
using LotusCore.EngineEvents;

namespace LotusCore.EngineEvents;

public class IntResult : EngineEventResult
{
    public int _result;

    public IntResult(int result)
    {
        _result = result;
    }
}

public class BoolResult : EngineEventResult
{
    public bool _result;

    public BoolResult(bool result)
    {
        _result = result;
    }
}

public class GuidResult : EngineEventResult
{
    public Guid? _result;

    public GuidResult(Guid? result)
    {
        _result = result;
    }
}

public class IpAddressResult : EngineEventResult
{
    public IPAddress? _result;

    public IpAddressResult(IPAddress? result)
    {
        _result = result;
    }
}
