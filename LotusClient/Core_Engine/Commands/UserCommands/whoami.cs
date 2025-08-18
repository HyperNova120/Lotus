using Core_Engine.Interfaces;
using Core_Engine.Modules.MojangLogin;

namespace Core_Engine.Commands.UserCommands
{
    public class Whoami : ICommandBase
    {
        public string GetCommandDescription()
        {
            return "Prints the Username of the minecraft account currently signed in";
        }

        public Task ProcessCommand(string[] commandArgs)
        {
            MojangLogin mojangLoginModule = Core_Engine.GetModule<MojangLogin>("MojangLogin")!;
            if (mojangLoginModule._UserProfile != null)
            {
                Console.WriteLine(mojangLoginModule._UserProfile.name);
                return Task.CompletedTask;
            }
            Console.WriteLine("You are not signed in");
            return Task.CompletedTask;
        }
    }
}
