using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;
using LotusCore.BaseClasses.World.Entities;

namespace LotusCore.Interfaces;

public interface IEntityModule : IModuleBase
{
    public void AddEntity(Entity entity);

    public void SetEntityAnimation(int entityID, EntityAnimation animation);

    public enum EntityAnimation
    {
        SwingMainArm,
        LeaveBed,
        SwingOffhand,
        CriticalEffect,
        MagicCriticalEffect,
    }

    public void EntityDamageEvent();

    public void EntityEvent(int entityID, byte entityStatus);

    public void TeleportEntity(
        int entityID,
        Position pos,
        Velocity vel,
        float yaw,
        float pitch,
        bool onGround
    );

    public void HurtAnimation(int entityID, float yaw);

    public void UpdateEntityPosition(
        int entityID,
        short deltaX,
        short deltaY,
        short deltaZ,
        bool onGround
    );

    public void UpdateEntityPositionAndRotation(
        int entityID,
        short deltaX,
        short deltaY,
        short deltaZ,
        Angle yaw,
        Angle pitch,
        bool onGround
    );

    public struct TrackSteps
    {
        double _x,
            _y,
            _z;
        double _velX,
            _velY,
            _velZ;
        Angle _yaw,
            _pitch;
        float _weight;
    }

    public void MoveMinecartAlongTrack(int entityID, IEnumerable<TrackSteps> steps);

    public void UpdateEntityRotation(int entityID, Angle yaw, Angle pitch, bool onGround);

    public void RemoveEntities(IEnumerable<int> entityIDs);

    public void RemoveEntityEffect(int entityID, int effectID);

    public void SetHeadRotation(int entityID, Angle headYaw);

    //public void LinkEntities();

    //public void SetEntityVelocity();

    //public void SetEquipment();

    public void SetPassengers(int entityID, IEnumerable<int> passangers);

    //public void PickupItem();

    //public void SynchronizeVehiclePosition();

    //public void UpdateAttributes();

    public void EntityEffect(int entityID, int effectID, int amplifier, int duration, byte flags);

    public enum EntityEffectFlags
    {
        IS_AMBIENT = 0x1,
        SHOW_PARTICLES = 0x2,
        SHOW_ICON = 0x4,
        BLEND = 0x8,
    }

    public void ProjectilePower(int entityID, double power);
}
