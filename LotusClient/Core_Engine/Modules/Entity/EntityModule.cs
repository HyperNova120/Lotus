using System.Security.Cryptography;
using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;
using LotusCore.BaseClasses.World.Entities;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;

namespace LotusCore.Modules.EntitiesModule;

public class EntityModule : IEntityModule
{
    Dictionary<int, Entity> _trackedEntities = new();

    public void AddEntity(Entity entity)
    {
        lock (_trackedEntities)
        {
            _trackedEntities[entity._EntityID] = entity;
        }
    }

    public void EntityDamageEvent()
    {
        throw new NotImplementedException();
    }

    public void EntityEffect(int entityID, int effectID, int amplifier, int duration, byte flags)
    {
        throw new NotImplementedException();
    }

    public void EntityEvent(int entityID, byte entityStatus)
    {
        throw new NotImplementedException();
    }

    public void HurtAnimation(int entityID, float yaw)
    {
        throw new NotImplementedException();
    }

    public void LinkModules(ICoreModule coreModule) { }

    public void MoveMinecartAlongTrack(int entityID, IEnumerable<IEntityModule.TrackSteps> steps)
    {
        throw new NotImplementedException();
    }

    public void ProjectilePower(int entityID, double power)
    {
        throw new NotImplementedException();
    }

    public void RegisterCommands(Action<string, ICommandBase> RegisterCommand) { }

    public void RegisterEvents(Action<string> RegisterEvent) { }

    public void RemoveEntities(IEnumerable<int> entityIDs)
    {
        for (int i = 0; i < entityIDs.Count(); i++)
        {
            lock (_trackedEntities)
            {
                _trackedEntities.Remove(entityIDs.ElementAt(i));
            }
        }
    }

    public void RemoveEntityEffect(int entityID, int effectID)
    {
        throw new NotImplementedException();
    }

    public void SetEntityAnimation(int entityID, IEntityModule.EntityAnimation animation)
    {
        throw new NotImplementedException();
    }

    public void SetHeadRotation(int entityID, Angle headYaw)
    {
        throw new NotImplementedException();
    }

    public void SetPassengers(int entityID, IEnumerable<int> passangers)
    {
        throw new NotImplementedException();
    }

    public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent) { }

    public void TeleportEntity(
        int entityID,
        Position pos,
        Velocity vel,
        float yaw,
        float pitch,
        bool onGround
    )
    {
        throw new NotImplementedException();
    }

    public void UpdateEntityPosition(
        int entityID,
        short deltaX,
        short deltaY,
        short deltaZ,
        bool onGround
    )
    {
        Entity entity = _trackedEntities[entityID];
        entity._Position!._X += deltaX;
        entity._Position._Y += deltaY;
        entity._Position._Z += deltaZ;
    }

    public void UpdateEntityPositionAndRotation(
        int entityID,
        short deltaX,
        short deltaY,
        short deltaZ,
        Angle yaw,
        Angle pitch,
        bool onGround
    )
    {
        throw new NotImplementedException();
    }

    public void UpdateEntityRotation(int entityID, Angle yaw, Angle pitch, bool onGround)
    {
        throw new NotImplementedException();
    }
}
