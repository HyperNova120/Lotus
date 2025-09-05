using LotusCore.EngineEventArgs;

namespace LotusCore.EngineEvents;

public delegate EngineEventResult? EngineEventHandler(object? sender, IEngineEventArgs args);
