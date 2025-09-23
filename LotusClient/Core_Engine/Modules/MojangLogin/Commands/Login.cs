using System.Threading.Tasks;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;
using LotusCore.Modules.MojangLogin.Models;
using LotusCore.Modules.MojangLogin.Types;

namespace LotusCore.Modules.MojangLogin.Commands
{
    public class LoginCommand : ICommandBase
    {
        public string GetCommandDescription()
        {
            return "Performs the login sequence to log into a Minecraft account";
        }

        public string GetCommandCorrectUsage()
        {
            return "Correct Usage: 'login'";
        }

        public async Task ProcessCommand(string[] commandArgs)
        {
            var userProfile = Core_Engine
                .InvokeEvent<UserProfileResult>("MOJANGLOGIN_GetUserProfile", null)!
                ._userProfile;
            if (userProfile != null)
            {
                Console.WriteLine("User Already Signed into Account " + userProfile.name);
                return;
            }

            Core_Engine.SignalInteractiveHold(Core_Engine.State.AccountLogin);
            bool sucessfullSignIn = Core_Engine
                .InvokeEvent<BoolResult>("MOJANGLOGIN_LoginAsync", null)!
                ._result;
            userProfile = Core_Engine
                .InvokeEvent<UserProfileResult>("MOJANGLOGIN_GetUserProfile", null)!
                ._userProfile;
            if (!sucessfullSignIn)
            {
                Logging.LogError("Failed to sign in");
            }
            else
            {
                Logging.LogInfo("Signed in as " + userProfile!.name);
            }
        }
    }
}
