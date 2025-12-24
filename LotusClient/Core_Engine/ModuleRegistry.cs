using LotusCore.Interfaces;

namespace LotusCore;

public static class ModuleRegistry
{
    static Dictionary<Type, IModuleBase> _registeredModules = new();

    public static T GetModule<T>()
        where T : IModuleBase
    {
        Type type = typeof(T);
        return (T)_registeredModules[type];
    }

    public static bool RegisterModule<T>(T module)
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

    public static void LinkModule<T>()
        where T : IModuleBase
    {
        GetModule<T>().LinkModules();
    }

    public static void LinkAllModules()
    {
        foreach (IModuleBase module in _registeredModules.Values)
        {
            module.LinkModules();
        }
    }

    public static void SubscribeAllModulesToEvents()
    {
        foreach (IModuleBase module in _registeredModules.Values)
        {
            module.SubscribeToEvents(Core_Engine.SubscribeToEvent);
        }
    }
}
