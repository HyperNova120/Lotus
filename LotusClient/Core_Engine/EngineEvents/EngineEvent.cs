using LotusCore.EngineEventArgs;

namespace LotusCore.EngineEvents;

public delegate EngineEventResult? EngineEventHandler(object? sender, IEngineEventArgs args);
public delegate EngineEventResult? EngineEventHandler<T>(object? sender, T args)
    where T : IEngineEventArgs;
public delegate T2? EngineEventHandler<T, T2>(object? sender, T args)
    where T : IEngineEventArgs
    where T2 : EngineEventResult;
