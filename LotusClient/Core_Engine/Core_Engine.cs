using System.Reflection;
using System.Threading.Tasks;
using Core_Engine.Commands;
using Core_Engine.Commands.UserCommands;
using Core_Engine.Exceptions;
using Core_Engine.Interfaces;
using Core_Engine.Modules.GameStateHandler;
using Core_Engine.Modules.MojangLogin;
using Core_Engine.Modules.Networking;
using Core_Engine.Modules.ServerConfig;
using Core_Engine.Modules.ServerLogin;
using Core_Engine.Modules.ServerStatus;
using Graphics_Engine;
using Org.BouncyCastle.Asn1.Ocsp;
using Silk.NET.Vulkan;

namespace Core_Engine
{
    public static class Core_Engine
    {
        private static Dictionary<string, ICommandBase> _Commands = new();
        private static Dictionary<string, IModuleBase> _Modules = new();
        private static Dictionary<string, IGraphicsModule> _GraphicsModules = new();
        private static Dictionary<string, EventHandler> _Events = new();

        public static State _CurrentState { private set; get; } = State.Noninteractive;

        static ManualResetEventSlim _InteractiveHold = new(true); // Initially signaled

        private static HashSet<State> _BlockingStates = new();

        private static bool _IsInteractiveHoldBlocking = false;

        public enum State
        {
            Interactive,
            Noninteractive,
            Waiting,
            Graphics,
            AccountLogin,
            JoiningServer,
            Configuration,
            Play,
        }

        public static bool SignalInteractiveHold(State RequestedState)
        {
            Logging.LogDebug($"signalInteractiveHold From:{RequestedState}");
            if (_BlockingStates.Contains(RequestedState))
            {
                Logging.LogDebug($"\tFAIL");
                return false;
            }
            if (_InteractiveHold.IsSet != _IsInteractiveHoldBlocking)
            {
                //if not blocking block
                _InteractiveHold.Reset();
            }
            _BlockingStates.Add(RequestedState);
            Logging.LogDebug($"\tPASS; Current Number of Blocking States:{_BlockingStates.Count}");
            return true;
        }

        public static bool signalInteractiveTransferHold(State CallingState, State RequestedState)
        {
            Logging.LogDebug(
                $"signalInteractiveTransferHold From:{CallingState} To:{RequestedState}"
            );
            if (!_BlockingStates.Contains(CallingState))
            {
                Logging.LogDebug($"\tFAIL; CallingState does not posses a hold");
                return false;
            }
            _BlockingStates.Add(RequestedState);
            _BlockingStates.Remove(CallingState);
            return true;
        }

        public static bool SignalInteractiveFree(State CallingState)
        {
            Logging.LogDebug($"signalInteractiveFree From:{CallingState}");
            if (!_BlockingStates.Contains(CallingState))
            {
                //not blocking from calling state
                Logging.LogDebug($"\tFAIL");
                return false;
            }

            _BlockingStates.Remove(CallingState);
            Logging.LogDebug($"\tPASS; Current Number of Blocking States:{_BlockingStates.Count}");

            if (_BlockingStates.Count == 0)
            {
                _CurrentState = State.Interactive;
                _InteractiveHold.Set();
            }
            return true;
        }

        public static void SignalInteractiveResetServerHolds()
        {
            if (_BlockingStates.Contains(State.JoiningServer))
                SignalInteractiveFree(State.JoiningServer);
            if (_BlockingStates.Contains(State.Configuration))
                SignalInteractiveFree(State.Configuration);
            if (_BlockingStates.Contains(State.Play))
                SignalInteractiveFree(State.Play);
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
            RegisterModule("GameStateHandler", new GameStateHandler());
            RegisterModule("MojangLogin", new MojangLogin());
            RegisterModule("Networking", new Networking());
            RegisterModule("ServerStatus", new ServerStatus());
            RegisterModule("LoginHandler", new LoginHandler());
            RegisterModule("VulkanGraphics", new VulkanGraphics());
            RegisterModule("ServerConfiguration", new ServerConfiguration());
        }

        private static void InitCoreModuleEventSubscriptions()
        {
            foreach (IModuleBase module in _Modules.Values)
            {
                module.SubscribeToEvents(SubscribeToEvent);
            }
        }

        //=========END===========
        //Initiate Core Engine
        //========================

        public static T? GetModule<T>(string ModuleIdentifier)
        {
            if (!_Modules.ContainsKey(ModuleIdentifier))
            {
                throw new Exceptions.IdentifierNotFoundException(
                    $"Module {ModuleIdentifier} Does Not Exist"
                );
            }
            _Modules.TryGetValue(ModuleIdentifier, out IModuleBase? moduleBase);
            return (T?)moduleBase;
        }

        public static async Task GoInteractiveMode()
        {
            _CurrentState = State.Interactive;
            bool shouldRun = true;
            _InteractiveHold.Set();
            while (_CurrentState == State.Interactive)
            {
                string userResponse = ConsoleUtils.AskUserLineResponseQuestion("Core Engine");
                string[] tokens = userResponse.Split(" ");
                string command = tokens[0].ToLower();

                try
                {
                    if (CheckAndRunInteractivityCommand(command, ref shouldRun))
                    {
                        if (!shouldRun)
                        {
                            break;
                        }
                    }
                    else
                    {
                        await HandleCommand(command, (tokens.Length > 1) ? [.. tokens[1..]] : []);
                        _InteractiveHold.Wait();
                    }
                }
                catch (Exception e)
                {
                    Logging.LogError(e.ToString());
                }
            }
            Console.WriteLine("Interactive Mode Ended");
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
                    foreach (string CommandIdentifier in _Commands.Keys)
                    {
                        Console.WriteLine(
                            $"\t{CommandIdentifier} -{_Commands[CommandIdentifier].GetCommandDescription()}"
                        );
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        public static async Task HandleCommand(string command, string[] args)
        {
            if (!_Commands.ContainsKey(command))
            {
                Console.WriteLine("Unknown Command, use 'help' to see a list of commands");
                return;
            }
            await _Commands[command].ProcessCommand(args);
        }

        public static void InvokeEvent(string EventIdentifier, EventArgs args)
        {
            if (!_Events.ContainsKey(EventIdentifier))
            {
                throw new IdentifierNotFoundException(
                    $"Event {EventIdentifier} has not been registered"
                );
            }
            if (_Events[EventIdentifier] == null)
            {
                throw new IdentifierNotFoundException($"Event {EventIdentifier} null");
                return;
            }
            _Events[EventIdentifier].Invoke(null, args);
        }

        //=========START===========
        //Register and Unregister
        //========================

        public static void RegisterCommand(string CommandIdentifier, ICommandBase CommandToRegister)
        {
            if (_Commands.ContainsKey(CommandIdentifier))
            {
                throw new Exceptions.IdentifierMustBeUniqueException(
                    $"Command Identifier {CommandIdentifier} Already Exists"
                );
            }
            _Commands.Add(CommandIdentifier, CommandToRegister);
        }

        public static bool UnregisterCommand(string CommandIdentifier)
        {
            if (!_Commands.ContainsKey(CommandIdentifier))
            {
                return false;
            }
            _Commands.Remove(CommandIdentifier);
            return true;
        }

        public static void RegisterModule(string ModuleIdentifier, IModuleBase ModuleToRegister)
        {
            if (_Modules.ContainsKey(ModuleIdentifier))
            {
                throw new Exceptions.IdentifierMustBeUniqueException(
                    $"Module Identifier {ModuleIdentifier} Already Exists"
                );
            }
            ModuleToRegister.RegisterEvents(RegisterEvent);
            ModuleToRegister.RegisterCommands(RegisterCommand);
            _Modules.Add(ModuleIdentifier, ModuleToRegister);
            if (ModuleToRegister is IGraphicsModule graphicsModule)
            {
                _GraphicsModules.Add(ModuleIdentifier, graphicsModule);
            }
        }

        public static bool UnregisterModule(string ModuleIdentifier)
        {
            if (!_Modules.ContainsKey(ModuleIdentifier))
            {
                return false;
            }
            _Modules.Remove(ModuleIdentifier);
            if (_GraphicsModules.ContainsKey(ModuleIdentifier))
            {
                _GraphicsModules.Remove(ModuleIdentifier);
            }
            return true;
        }

        public static void RegisterEvent(string EventIdentifier)
        {
            if (_Events.ContainsKey(EventIdentifier))
            {
                throw new Exceptions.IdentifierMustBeUniqueException(
                    $"Event Identifier {EventIdentifier} Already Exists"
                );
            }
            _Events.Add(EventIdentifier, null);
        }

        public static void SubscribeToEvent(string EventIdentifier, EventHandler callback)
        {
            if (!_Events.ContainsKey(EventIdentifier))
            {
                throw new Exceptions.IdentifierNotFoundException(
                    $"Event {EventIdentifier} Does Not Exist"
                );
            }
            if (_Events[EventIdentifier] == null)
            {
                _Events[EventIdentifier] = callback;
                return;
            }
            _Events[EventIdentifier] += callback;
        }

        public static void UnsubscribeToEvent(string EventIdentifier, EventHandler callback)
        {
            if (!_Events.ContainsKey(EventIdentifier))
            {
                throw new Exceptions.IdentifierNotFoundException(
                    $"Event {EventIdentifier} Does Not Exist"
                );
            }
            _Events[EventIdentifier] -= callback;
        }

        //==========END===========
        //Register and Unregister
        //========================
    }
}
