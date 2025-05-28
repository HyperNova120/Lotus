using System.Threading.Tasks;
using Core_Engine.Interfaces;
using Core_Engine.Modules.MojangLogin.Models;

namespace Core_Engine.Modules.MojangLogin.Commands
{
    public class LoginCommand : ICommandBase
    {
        public string GetCommandDescription()
        {
            return "Performs the login sequence to log into a Minecraft account";
        }

        public async Task ProcessCommand(string[] commandArgs)
        {
            var mojangLogin = Core_Engine.GetModule<MojangLogin>("MojangLogin")!;
            if (mojangLogin.userProfile != null)
            {
                Console.WriteLine(
                    "User Already Signed into Account " + mojangLogin.userProfile.name
                );
                return;
            }
            await mojangLogin.LoginAsync();
            if (mojangLogin.userProfile == null)
            {
                Logging.LogError("Failed to sign in");
            }
            else
            {
                Logging.LogInfo("Signed in as " + mojangLogin.userProfile!.name);
            }
        }
    }
}
