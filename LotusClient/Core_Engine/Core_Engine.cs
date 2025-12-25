using LotusCore.Commands.UserCommands;
using LotusCore.Interfaces;
using LotusCore.Modules.Chat;
using LotusCore.Modules.EntitiesModule;
using LotusCore.Modules.GameStateHandlerModule;
using LotusCore.Modules.LotusNetty;
using LotusCore.Modules.MojangLogin;
using LotusCore.Modules.ServerConfig;
using LotusCore.Modules.ServerList;
using LotusCore.Modules.ServerLogin;
using LotusCore.Modules.ServerPlay;
using LotusCore.Utils.MinecraftPaths;

namespace LotusCore;

public class Core_Engine : CoreModule, ICoreModule
{
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
        ServerList,
    }

    public bool SignalInteractiveHold(State RequestedState)
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

    public bool SignalInteractiveHoldTransfer(State CallingState, State RequestedState)
    {
        Logging.LogDebug($"signalInteractiveTransferHold From:{CallingState} To:{RequestedState}");
        if (!_BlockingStates.Contains(CallingState))
        {
            Logging.LogDebug($"\tFAIL; CallingState does not posses a hold");
            return false;
        }
        _BlockingStates.Add(RequestedState);
        _BlockingStates.Remove(CallingState);
        return true;
    }

    public bool SignalInteractiveFree(State CallingState)
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

    public void SignalInteractiveResetServerHolds()
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

    public void InitCore()
    {
        MinecraftPathsStruct.InitRequiredFolderStructure();
        InitCoreCommands();
        InitCoreModules();
        InitCoreModuleEventSubscriptions();
    }

    private void InitCoreCommands()
    {
        RegisterCommand("whoami", new Whoami());
    }

    private void InitCoreModules()
    {
        RegisterModule<IMojangLoginModule>(new MojangLogin());
        RegisterModule<IGameStateHandlerModule>(new GameStateHandler());
        RegisterModule<INetworkModule>(new Networking());
        RegisterModule<IServerListModule>(new ServerList());
        RegisterModule(new LoginHandler());
        RegisterModule(new ServerConfiguration());
        RegisterModule<IServerPlayHandlerModule>(new ServerPlayHandler());
        RegisterModule<IServerChatModule>(new ServerChat());
        RegisterModule<IEntityModule>(new EntityModule());

        LinkAllModules();
    }

    private void InitCoreModuleEventSubscriptions()
    {
        SubscribeAllModulesToEvents();
    }

    //=========END===========
    //Initiate Core Engine
    //========================

    /* public T? GetModule<T>()
        where T : IModuleBase
    {
        return GetModule<T>();
    } */

    public async Task GoInteractiveMode(IEnumerable<string>? initialCmds = null)
    {
        initialCmds ??= [];
        int initCmdIndex = 0;
        _CurrentState = State.Interactive;
        bool shouldRun = true;
        _InteractiveHold.Set();
        while (_CurrentState == State.Interactive)
        {
            string userResponse =
                (initCmdIndex < initialCmds.Count())
                    ? initialCmds.ElementAt(initCmdIndex++)
                    : ConsoleUtils.AskUserLineResponseQuestion("Core Engine");
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

    private bool CheckAndRunInteractivityCommand(string command, ref bool shouldRun)
    {
        switch (command)
        {
            case string str when str == "exit" || str == "quit":
                shouldRun = false;
                break;
            case "help":
                Console.WriteLine("Available Commands Are");
                int descIndent = 2;
                foreach (string CommandIdentifier in _commands.Keys)
                {
                    int numIndentDecrement = CommandIdentifier.Length / 7;
                    Console.WriteLine(
                        $"\t{CommandIdentifier} {new string('\t', descIndent - numIndentDecrement)}-{_commands[CommandIdentifier].GetCommandDescription()}"
                    );
                }
                break;
            default:
                return false;
        }
        return true;
    }

    /* public static async Task HandleCommand(string command, string[] args)
    {
        if (!_Commands.ContainsKey(command.ToLower()))
        {
            Console.WriteLine($"Unknown Command '{command}', use 'help' to see a list of commands");
            //Logging.LogError("", true);
            return;
        }
        await _Commands[command].ProcessCommand(args);
    } */

    /*  private static T? InvokeEvent<T>(string EventIdentifier, IEngineEventArgs? args)
         where T : EngineEventResult
     {
         if (!_Events.ContainsKey(EventIdentifier))
         {
             throw new IdentifierNotFoundException(
                 $"Event {EventIdentifier} has not been registered"
             );
         }
         if (_Events[EventIdentifier] == null)
         {
             //throw new IdentifierNotFoundException($"Event {EventIdentifier} null");
             Logging.LogError($"Event {EventIdentifier} null");
             return null;
         }
         try
         {
             return (T?)_Events[EventIdentifier].Invoke(null, args);
         }
         catch (Exception e)
         {
             Logging.LogError($"CoreEngine.InvokeEvent: EventIdentifier: {EventIdentifier}, \n{e}");
             return null;
         }
     } */

    /* private static T? InvokeEvent<T>(string EventIdentifier)
        where T : EngineEventResult
    {
        return InvokeEvent<T>(EventIdentifier, null);
    }

    public static void InvokeEvent(string EventIdentifier, IEngineEventArgs? args)
    {
        if (!_Events.ContainsKey(EventIdentifier))
        {
            throw new IdentifierNotFoundException(
                $"Event {EventIdentifier} has not been registered"
            );
        }
        if (_Events[EventIdentifier] == null)
        {
            //throw new IdentifierNotFoundException($"Event {EventIdentifier} null");
            Logging.LogError($"Event {EventIdentifier} null");
            return;
        }
        _Events[EventIdentifier].Invoke(null, args);
    }

    public static void InvokeEvent(string EventIdentifier)
    {
        InvokeEvent(EventIdentifier, null);
    } */

    //=========START===========
    //Register and Unregister
    //========================

    /* public static void RegisterCommand(string CommandIdentifier, ICommandBase CommandToRegister)
    {
        //Logging.LogDebug("REGISTER COMMAND: " + CommandIdentifier);
        if (_Commands.ContainsKey(CommandIdentifier))
        {
            throw new Exceptions.IdentifierMustBeUniqueException(
                $"Command Identifier {CommandIdentifier} Already Exists"
            );
        }
        _Commands.Add(CommandIdentifier.ToLower(), CommandToRegister);
    }

    public static bool UnregisterCommand(string CommandIdentifier)
    {
        if (!_Commands.ContainsKey(CommandIdentifier))
        {
            return false;
        }
        _Commands.Remove(CommandIdentifier);
        return true;
    } */

    /* private static void RegisterModule<T>(T module)
        where T : IModuleBase
    {
        _moduleRegistry.RegisterModule(module);
    }

    public static void RegisterAndLinkModule<T>()
        where T : IModuleBase
    {
        T module = _moduleRegistry.GetModule<T>();
        module.LinkModules(this);
    } */

    /* public static bool UnregisterModule(string ModuleIdentifier)
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
 */
    /* public static void RegisterEvent(string EventIdentifier)
    {
        if (_Events.ContainsKey(EventIdentifier))
        {
            throw new Exceptions.IdentifierMustBeUniqueException(
                $"Event Identifier {EventIdentifier} Already Exists"
            );
        }
        _Events.Add(EventIdentifier, null);
    }

    public static void SubscribeToEvent(string EventIdentifier, EngineEventHandler callback)
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

    public static void UnsubscribeToEvent(string EventIdentifier, EngineEventHandler callback)
    {
        if (!_Events.ContainsKey(EventIdentifier))
        {
            throw new Exceptions.IdentifierNotFoundException(
                $"Event {EventIdentifier} Does Not Exist"
            );
        }
        _Events[EventIdentifier] -= callback;
    } */

    //==========END===========
    //Register and Unregister
    //========================
}
