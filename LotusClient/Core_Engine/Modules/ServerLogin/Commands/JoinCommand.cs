using LotusCore.Interfaces;

namespace LotusCore.Modules.ServerLogin.Commands
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
            if (networking._IsClientConnectedToPrimaryServer)
            {
                Console.WriteLine("you are already connected to a server");
                return;
            }
            //Core_Engine.CurrentState = Core_Engine.State.Waiting;
            Core_Engine.SignalInteractiveHold(Core_Engine.State.JoiningServer);
            Logging.LogInfo("Attempting to connect to server");
            if (commandArgs.Length == 1)
            {
                loginHandler.LoginToServer(commandArgs[0]);
            }
            else
            {
                loginHandler.LoginToServer(commandArgs[0], ushort.Parse(commandArgs[1]));
            }
        }
    }
}
