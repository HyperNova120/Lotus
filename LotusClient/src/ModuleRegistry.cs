using LotusCore.Interfaces;

namespace LotusCore;

public class ModuleRegistry
{
    Dictionary<Type, IModuleBase> _registeredModules = new();

    public T? GetModule<T>()
        where T : IModuleBase
    {
        Type type = typeof(T);
        if (!_registeredModules.ContainsKey(type))
        {
            return default;
        }
        return (T)_registeredModules[type];
    }

    public bool RegisterModule<T>(T module)
        where T : IModuleBase
    {
        Type type = typeof(T);
        if (_registeredModules.ContainsKey(type))
        {
            return false;
        }
        lock (_registeredModules)
        {
            _registeredModules[type] = module;
        }
        return true;
    }

    public bool RegisterModule<T>()
        where T : IModuleBase, new()
    {
        Type type = typeof(T);
        if (_registeredModules.ContainsKey(type))
        {
            return false;
        }
        lock (_registeredModules)
        {
            _registeredModules[type] = new T();
        }
        return true;
    }

    public void LinkModule<T>(ICoreModule coreModule)
        where T : IModuleBase
    {
        GetModule<T>()?.LinkModules(coreModule);
    }

    public void LinkAllModules(ICoreModule coreModule)
    {
        foreach (IModuleBase module in _registeredModules.Values)
        {
            module.LinkModules(coreModule);
        }
    }

    public void SubscribeAllModulesToEvents(ICoreModule coreModule)
    {
        foreach (IModuleBase module in _registeredModules.Values)
        {
            module.SubscribeToEvents(coreModule.SubscribeToEvent);
        }
    }
}
