using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;

namespace LotusCore.Modules.ServerLogin.Commands
{
    public class JoinCommand : ICommandBase
    {
        public string GetCommandDescription()
        {
            return "Attempts to join the provided server and (optional)IP";
        }

        public string GetCommandCorrectUsage()
        {
            return "Correct usage: 'join <server/ip> <port>(optional); ex join play.hypixel.net'";
        }

        public async Task ProcessCommand(string[] commandArgs)
        {
            /* ServerListServerIPResult? result = (ServerListServerIPResult?)
                Core_Engine.InvokeEvent(
                    "ServerListIP_Request",
                    new ServerListIPRequestEventArgs("Wynncraft")
                );

            string resultString = (result == null) ? "NULL" : $"{result._ip}:{result._port}";
            Logging.LogDebug($"Try Use Return: {resultString}"); */

            if (commandArgs.Length < 1 || commandArgs.Length > 3)
            {
                Console.WriteLine(GetCommandCorrectUsage());
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

            bool isTransfer = false;
            List<string> tmp = new();
            foreach (string s in commandArgs)
            {
                if (s == "-t")
                {
                    isTransfer = true;
                }
                else
                {
                    tmp.Add(s);
                }
            }

            commandArgs = tmp.ToArray();
            if (commandArgs.Length == 1)
            {
                loginHandler.LoginToServer(commandArgs[0], isTransfer);
            }
            else
            {
                loginHandler.LoginToServer(
                    commandArgs[0],
                    isTransfer,
                    ushort.Parse(commandArgs[1])
                );
            }
        }
    }
}
