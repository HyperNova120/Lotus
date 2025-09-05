using LotusCore.EngineEvents;

namespace LotusCore.Interfaces
{
    public interface IModuleBase
    {
        /// <summary>
        /// Register Events that you invoke/send out
        /// </summary>
        /// <param name="RegisterEvent"></param>
        public void RegisterEvents(Action<string> RegisterEvent);

        /// <summary>
        /// subscribe to events that another module has registered
        /// </summary>
        /// <param name="SubscribeToEvent"></param>
        public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent);

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand);
    }
}
