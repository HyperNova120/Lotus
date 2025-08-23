namespace LotusCore.Interfaces
{
    public interface IModuleBase
    {
        /// <summary>
        /// Register Events that you take
        /// </summary>
        /// <param name="RegisterEvent"></param>
        public void RegisterEvents(Action<string> RegisterEvent);

        /// <summary>
        /// subscribe to events that another module has registered
        /// </summary>
        /// <param name="SubscribeToEvent"></param>
        public void SubscribeToEvents(Action<string, EventHandler> SubscribeToEvent);

        public void RegisterCommands(Action<string, ICommandBase> RegisterCommand);
    }
}
