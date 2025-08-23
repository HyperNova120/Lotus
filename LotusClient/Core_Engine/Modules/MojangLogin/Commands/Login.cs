using System.Threading.Tasks;
using LotusCore.Interfaces;
using LotusCore.Modules.MojangLogin.Models;

namespace LotusCore.Modules.MojangLogin.Commands
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
            if (mojangLogin._UserProfile != null)
            {
                Console.WriteLine(
                    "User Already Signed into Account " + mojangLogin._UserProfile.name
                );
                return;
            }

            Core_Engine.SignalInteractiveHold(Core_Engine.State.AccountLogin);
            await mojangLogin.LoginAsync();
            if (mojangLogin._UserProfile == null)
            {
                Logging.LogError("Failed to sign in");
            }
            else
            {
                Logging.LogInfo("Signed in as " + mojangLogin._UserProfile!.name);
            }
        }
    }
}
