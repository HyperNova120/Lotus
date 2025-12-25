using LotusCore.EngineEvents;
using LotusCore.Interfaces;

public class GraphicsEngine : CoreModule, IModuleBase
{
    public void LinkModules(ICoreModule coreModule) { }

    public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

    public void RegisterEvents(Action<string> RegisterEvent) { }

    public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent) { }
}
