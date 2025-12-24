using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;
using LotusCore.EngineEvents;
using LotusCore.Interfaces;

namespace LotusCore.Modules.Entity;

public class EntityModule : IEntityModule
{
    public void AddEntity(BaseClasses.World.Entities.Entity entity)
    {
        throw new NotImplementedException();
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

    public void LinkModules()
    {
        throw new NotImplementedException();
    }

    public void MoveMinecartAlongTrack(int entityID, IEnumerable<IEntityModule.TrackSteps> steps)
    {
        throw new NotImplementedException();
    }

    public void ProjectilePower(int entityID, double power)
    {
        throw new NotImplementedException();
    }

    public void RegisterCommands(Action<string, ICommandBase> RegisterCommand)
    {
        throw new NotImplementedException();
    }

    public void RegisterEvents(Action<string> RegisterEvent)
    {
        throw new NotImplementedException();
    }

    public void RemoveEntities(IEnumerable<int> entityIDs)
    {
        throw new NotImplementedException();
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

    public void SubscribeToEvents(Action<string, EngineEventHandler> SubscribeToEvent)
    {
        throw new NotImplementedException();
    }

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
        throw new NotImplementedException();
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
