using LotusCore.BaseClasses.World;
using LotusCore.BaseClasses.World.Entities;

namespace LotusCore.Modules.GameStateHandlerModule.Internals.EntityStuff;

public class EntityHandler
{
    /// <summary>
    /// Entity ID to Entity map
    /// </summary>
    Dictionary<int, Entity> _entities = new();

    public Entity? TryGetEntity(int EntityID)
    {
        _entities.TryGetValue(EntityID, out Entity? returner);
        return returner;
    }

    public void AddEntity(Entity entity)
    {
        RemoveEntity(entity);
        lock (_entities)
        {
            _entities.Add(entity._EntityID, entity);
        }
    }

    public void RemoveEntity(Entity entity)
    {
        lock (_entities)
        {
            _entities.Remove(entity._EntityID);
        }
    }
}
