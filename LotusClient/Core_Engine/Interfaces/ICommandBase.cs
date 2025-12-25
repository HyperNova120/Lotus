namespace LotusCore.Interfaces
{
    public interface ICommandBase
    {
        public Task ProcessCommand(ICoreModule coreMoudle, string[] commandArgs);

        public string GetCommandCorrectUsage();

        public string GetCommandDescription();
    }
}
