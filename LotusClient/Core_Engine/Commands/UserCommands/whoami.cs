using LotusCore.Interfaces;
using LotusCore.Modules.GameStateHandlerModule.Types;
using LotusCore.Modules.MojangLogin;
using LotusCore.Modules.MojangLogin.Models;

namespace LotusCore.Commands.UserCommands
{
    public class Whoami : ICommandBase
    {
        public string GetCommandDescription()
        {
            return "Prints the Username of the minecraft account currently signed in";
        }

        public string GetCommandCorrectUsage()
        {
            return "Correct Usage: 'whoami'";
        }

        public Task ProcessCommand(string[] commandArgs)
        {
            MinecraftProfile? userProfile = Core_Engine
                .InvokeEvent<MinecraftProfileResult>("GAMESTATE_GetUserProfile")!
                ._minecraftProfile;
            if (userProfile != null)
            {
                Console.WriteLine(userProfile.name);
                return Task.CompletedTask;
            }
            Console.WriteLine("You are not signed in");
            return Task.CompletedTask;
        }
    }
}
