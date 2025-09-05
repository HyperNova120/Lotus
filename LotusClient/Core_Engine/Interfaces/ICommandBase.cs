namespace LotusCore.Interfaces
{
    public interface ICommandBase
    {
        public Task ProcessCommand(string[] commandArgs);

        public string GetCommandHelpInfo();

        public string GetCommandDescription();
    }
}
