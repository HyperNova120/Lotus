using System.Reflection;
using System.Threading.Tasks;
using Core_Engine.Commands;
using Core_Engine.Commands.UserCommands;
using Core_Engine.Exceptions;
using Core_Engine.Interfaces;
using Core_Engine.Modules.MojangLogin;
using Core_Engine.Modules.Networking;
using Core_Engine.Modules.ServerLogin;
using Core_Engine.Modules.ServerStatus;
using Graphics_Engine;
using Silk.NET.Vulkan;

namespace Core_Engine
{
    public static class Core_Engine
    {
        private static Dictionary<string, ICommandBase> Commands = new();
        private static Dictionary<string, IModuleBase> Modules = new();
        private static Dictionary<string, EventHandler> Events = new();

        public static State CurrentState = State.Noninteractive;

        public enum State
        {
            Interactive,
            Noninteractive,
            Waiting,
            Graphics,
        }

        //private static State CurrentState = State.Noninteractive;

        //=========START===========
        //Initiate Core Engine
        //========================

        public static void InitCore()
        {
            InitCoreCommands();
            InitCoreModules();
            InitCoreModuleEventSubscriptions();
        }

        private static void InitCoreCommands()
        {
            RegisterCommand("whoami", new Whoami());
        }

        private static void InitCoreModules()
        {
            RegisterModule("MojangLogin", new MojangLogin());
            RegisterModule("Networking", new Networking());
            RegisterModule("ServerStatus", new ServerStatus());
            RegisterModule("LoginHandler", new LoginHandler());
            RegisterModule("VulkanGraphics", new VulkanGraphics());
        }

        private static void InitCoreModuleEventSubscriptions()
        {
            foreach (IModuleBase module in Modules.Values)
            {
                module.SubscribeToEvents(SubscribeToEvent);
            }
        }

        //=========END===========
        //Initiate Core Engine
        //========================

        public static T? GetModule<T>(string ModuleIdentifier)
        {
            if (!Modules.ContainsKey(ModuleIdentifier))
            {
                throw new Exceptions.IdentifierNotFoundException(
                    $"Module {ModuleIdentifier} Does Not Exist"
                );
            }
            Modules.TryGetValue(ModuleIdentifier, out IModuleBase? moduleBase);
            return (T?)moduleBase;
        }

        public static async Task GoInteractiveMode()
        {
            CurrentState = State.Interactive;
            bool shouldRun = true;
            while (CurrentState == State.Interactive)
            {
                string userResponse = ConsoleUtils.AskUserLineResponseQuestion("Core Engine");
                string[] tokens = userResponse.Split(" ");
                string command = tokens[0].ToLower();

                try
                {
                    if (!CheckAndRunInteractivityCommand(command, ref shouldRun))
                    {
                        await HandleCommand(command, (tokens.Length > 1) ? [.. tokens[1..]] : []);
                    }
                }
                catch (Exception e)
                {
                    Logging.LogError(e.ToString());
                }

                while (
                    GetModule<Networking>("Networking")!.IsClientConnectedToPrimaryServer
                    || CurrentState == State.Waiting
                )
                {
                    // Logging.LogDebug(
                    // $"IsClientConnectedToPrimaryServer:{GetModule<Networking>("Networking")!.IsClientConnectedToPrimaryServer} CurrentState:{CurrentState}"
                    //);
                    await Task.Delay(250);
                }
            }
            Console.WriteLine("Interactive Mode Ended");
            //CurrentState = State.Noninteractive;
        }

        private static bool CheckAndRunInteractivityCommand(string command, ref bool shouldRun)
        {
            switch (command)
            {
                case string str when str == "exit" || str == "quit":
                    shouldRun = false;
                    break;
                case "help":
                    Console.WriteLine("Available Commands Are");
                    foreach (string CommandIdentifier in Commands.Keys)
                    {
                        Console.WriteLine(
                            $"\t{CommandIdentifier} -{Commands[CommandIdentifier].GetCommandDescription()}"
                        );
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        private static async Task HandleCommand(string command, string[] args)
        {
            if (!Commands.ContainsKey(command))
            {
                Console.WriteLine("Unknown Command, use 'help' to see a list of commands");
                return;
            }
            await Commands[command].ProcessCommand(args);
        }

        public static void InvokeEvent(string EventIdentifier, EventArgs args)
        {
            if (!Events.ContainsKey(EventIdentifier))
            {
                throw new IdentifierNotFoundException(
                    $"Event {EventIdentifier} has not been registered"
                );
            }
            if (Events[EventIdentifier] == null)
            {
                return;
            }
            Events[EventIdentifier].Invoke(null, args);
        }

        //=========START===========
        //Register and Unregister
        //========================

        public static void RegisterCommand(string CommandIdentifier, ICommandBase CommandToRegister)
        {
            if (Commands.ContainsKey(CommandIdentifier))
            {
                throw new Exceptions.IdentifierMustBeUniqueException(
                    $"Command Identifier {CommandIdentifier} Already Exists"
                );
            }
            Commands.Add(CommandIdentifier, CommandToRegister);
        }

        public static bool UnregisterCommand(string CommandIdentifier)
        {
            if (!Commands.ContainsKey(CommandIdentifier))
            {
                return false;
            }
            Commands.Remove(CommandIdentifier);
            return true;
        }

        public static void RegisterModule(string ModuleIdentifier, IModuleBase ModuleToRegister)
        {
            if (Modules.ContainsKey(ModuleIdentifier))
            {
                throw new Exceptions.IdentifierMustBeUniqueException(
                    $"Module Identifier {ModuleIdentifier} Already Exists"
                );
            }
            ModuleToRegister.RegisterEvents(RegisterEvent);
            ModuleToRegister.RegisterCommands(RegisterCommand);
            Modules.Add(ModuleIdentifier, ModuleToRegister);
        }

        public static bool UnregisterModule(string ModuleIdentifier)
        {
            if (!Modules.ContainsKey(ModuleIdentifier))
            {
                return false;
            }
            Modules.Remove(ModuleIdentifier);
            return true;
        }

        public static void RegisterEvent(string EventIdentifier)
        {
            if (Events.ContainsKey(EventIdentifier))
            {
                throw new Exceptions.IdentifierMustBeUniqueException(
                    $"Event Identifier {EventIdentifier} Already Exists"
                );
            }
            Events.Add(EventIdentifier, null);
        }

        public static void SubscribeToEvent(string EventIdentifier, EventHandler callback)
        {
            if (!Events.ContainsKey(EventIdentifier))
            {
                throw new Exceptions.IdentifierNotFoundException(
                    $"Event {EventIdentifier} Does Not Exist"
                );
            }
            if (Events[EventIdentifier] == null)
            {
                Events[EventIdentifier] = callback;
                return;
            }
            Events[EventIdentifier] += callback;
        }

        public static void UnsubscribeToEvent(string EventIdentifier, EventHandler callback)
        {
            if (!Events.ContainsKey(EventIdentifier))
            {
                throw new Exceptions.IdentifierNotFoundException(
                    $"Event {EventIdentifier} Does Not Exist"
                );
            }
            Events[EventIdentifier] -= callback;
        }

        //==========END===========
        //Register and Unregister
        //========================
    }
}
