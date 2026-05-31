using System.Reflection.Metadata;
using LotusCore;
using LotusCore.EngineEventArgs;
using LotusCore.EngineEvents;
using LotusCore.Exceptions;
using LotusCore.Interfaces;
using Silk.NET.OpenAL;
using static LotusCore.Core_Engine;

public class CoreModule : ICoreModule
{
    internal ModuleRegistry _moduleRegistry = new();

    internal Dictionary<string, ICommandBase> _commands = new();

    internal Dictionary<string, EngineEventHandler> _events = new();

    //=============================
    //       Module Registry
    //=============================

    public T? GetModule<T>()
        where T : IModuleBase
    {
        return _moduleRegistry.GetModule<T>();
    }

    public bool RegisterModule<T>(T module)
        where T : IModuleBase
    {
        if (_moduleRegistry.RegisterModule<T>(module))
        {
            module.RegisterCommands(RegisterCommand);
            module.RegisterEvents(RegisterEvent);
            return true;
        }
        return false;
    }

    public void LinkModule<T>()
        where T : IModuleBase
    {
        _moduleRegistry.LinkModule<T>(this);
    }

    public void LinkAllModules()
    {
        _moduleRegistry.LinkAllModules(this);
    }

    public void SubscribeAllModulesToEvents()
    {
        _moduleRegistry.SubscribeAllModulesToEvents(this);
    }

    //=============================
    //          Commands
    //=============================

    public async Task HandleCommand(string command, string[] args)
    {
        if (!_commands.ContainsKey(command.ToLower()))
        {
            Console.WriteLine($"Unknown Command '{command}', use 'help' to see a list of commands");
            return;
        }
        await _commands[command].ProcessCommand(this, args);
    }

    public void RegisterCommand(string CommandIdentifier, ICommandBase CommandToRegister)
    {
        //Logging.LogDebug("REGISTER COMMAND: " + CommandIdentifier);
        if (_commands.ContainsKey(CommandIdentifier))
        {
            throw new LotusCore.Exceptions.IdentifierMustBeUniqueException(
                $"Command Identifier {CommandIdentifier} Already Exists"
            );
        }
        _commands.Add(CommandIdentifier.ToLower(), CommandToRegister);
    }

    public bool UnregisterCommand(string CommandIdentifier)
    {
        if (!_commands.ContainsKey(CommandIdentifier))
        {
            return false;
        }
        _commands.Remove(CommandIdentifier);
        return true;
    }

    //=============================
    //          Events
    //=============================

    public void InvokeEvent(string eventIdentifier)
    {
        InvokeEvent(eventIdentifier, null);
    }

    public void InvokeEvent(string eventIdentifier, IEngineEventArgs? args)
    {
        if (!_events.ContainsKey(eventIdentifier))
        {
            throw new IdentifierNotFoundException(
                $"Event {eventIdentifier} has not been registered"
            );
        }
        if (_events[eventIdentifier] == null)
        {
            //throw new IdentifierNotFoundException($"Event {EventIdentifier} null");
            Logging.LogError($"Event {eventIdentifier} null");
            return;
        }
        _events[eventIdentifier].Invoke(null, args);
    }

    public void RegisterEvent(string EventIdentifier)
    {
        if (_events.ContainsKey(EventIdentifier))
        {
            throw new IdentifierMustBeUniqueException(
                $"Event Identifier {EventIdentifier} Already Exists"
            );
        }
        _events.Add(EventIdentifier, null);
    }

    public void SubscribeToEvent(string EventIdentifier, EngineEventHandler callback)
    {
        if (!_events.ContainsKey(EventIdentifier))
        {
            throw new IdentifierNotFoundException($"Event {EventIdentifier} Does Not Exist");
        }
        if (_events[EventIdentifier] == null)
        {
            _events[EventIdentifier] = callback;
            return;
        }
        _events[EventIdentifier] += callback;
    }

    public void UnsubscribeToEvent(string EventIdentifier, EngineEventHandler callback)
    {
        if (!_events.ContainsKey(EventIdentifier))
        {
            throw new IdentifierNotFoundException($"Event {EventIdentifier} Does Not Exist");
        }
        _events[EventIdentifier] -= callback;
    }
}

public interface ICoreModule
{
    public T? GetModule<T>()
        where T : IModuleBase;

    public void SubscribeToEvent(string EventIdentifier, EngineEventHandler callback);
    public void InvokeEvent(string eventIdentifier);
    public void InvokeEvent(string eventIdentifier, IEngineEventArgs? args);

    public Task HandleCommand(string command, string[] args);

    public virtual bool SignalInteractiveHold(State requestedState)
    {
        throw new NotImplementedException();
    }

    public virtual bool SignalInteractiveHoldTransfer(State callingState, State requestedState)
    {
        throw new NotImplementedException();
    }

    public virtual bool SignalInteractiveFree(State callingState)
    {
        throw new NotImplementedException();
    }

    public virtual void SignalInteractiveResetServerHolds()
    {
        throw new NotImplementedException();
    }
}
