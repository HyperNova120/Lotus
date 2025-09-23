using LotusCore.BaseClasses;
using LotusCore.EngineEventArgs;

public class GuidEngineArgs : IEngineEventArgs
{
    public Guid _value;

    public GuidEngineArgs(Guid guid)
    {
        _value = guid;
    }
}

public class BoolEngineArgs : IEngineEventArgs
{
    public bool _value;

    public BoolEngineArgs(bool value)
    {
        _value = value;
    }
}

public class IdentifierEngineArgs : IEngineEventArgs
{
    public Identifier _value;

    public IdentifierEngineArgs(Identifier id)
    {
        _value = id;
    }
}

public class DateTimeEngineArgs : IEngineEventArgs
{
    public DateTime _value;

    public DateTimeEngineArgs(DateTime value)
    {
        _value = value;
    }
}
