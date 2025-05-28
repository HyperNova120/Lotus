namespace Core_Engine.Interfaces
{
    public interface ICommandBase
    {
        public Task ProcessCommand(string[] commandArgs);

        public string GetCommandDescription();
    }
}
