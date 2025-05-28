using Core_Engine.Interfaces;

namespace Core_Engine.Modules.ServerLogin.Commands
{
    public class JoinCommand : ICommandBase
    {
        public string GetCommandDescription()
        {
            return "Attempts to join the provided server and (optional)IP";
        }

        public async Task ProcessCommand(string[] commandArgs)
        {
            if (commandArgs.Length < 1 || commandArgs.Length > 2)
            {
                Console.WriteLine(
                    "Incorrect Usage; correct usage: join <server/ip> <port>(optional); ex join hypixel.net"
                );
                return;
            }
            LoginHandler loginHandler = Core_Engine.GetModule<LoginHandler>("LoginHandler")!;
            Networking.Networking networking = Core_Engine.GetModule<Networking.Networking>(
                "Networking"
            )!;
            if (networking.connectionState != Networking.Networking.ConnectionState.NONE)
            {
                Console.WriteLine("you are already connected to a server");
                return;
            }
            loginHandler.LoginToServer(commandArgs[0]);
        }
    }
}
