using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;

namespace LotusCore.Modules.ServerLogin.Commands
{
    public class ListJoinCommand : ICommandBase
    {
        public string GetCommandDescription()
        {
            return "Attempts to join the provided server from the server list";
        }

        public string GetCommandCorrectUsage()
        {
            return "Correct usage: 'listjoin <server list name>";
        }

        public async Task ProcessCommand(string[] commandArgs)
        {
            if (commandArgs.Length == 0)
            {
                Console.WriteLine(GetCommandCorrectUsage());
                return;
            }
            string serverListName = "";
            foreach (string s in commandArgs)
            {
                serverListName += (serverListName == "") ? s : $" {s}";
            }
            ServerListServerIPResult? result = (ServerListServerIPResult?)
                Core_Engine.InvokeEvent(
                    "ServerListIP_Request",
                    new ServerListIPRequestEventArgs(serverListName)
                );
            if (result == null || (result != null && result._ip == ""))
            {
                Console.WriteLine("Server not found in server list");
                return;
            }

            if (result._port == "")
            {
                await Core_Engine.HandleCommand("join", [result._ip]);
            }
            else
            {
                await Core_Engine.HandleCommand("join", [result._ip, result._port]);
            }

            /* string resultString = (result == null) ? "NULL" : $"{result._ip}:{result._port}";
            Logging.LogDebug($"Try Use Return: {resultString}");

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
            } */
        }
    }
}
