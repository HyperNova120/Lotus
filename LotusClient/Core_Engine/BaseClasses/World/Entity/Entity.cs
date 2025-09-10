using LotusCore.BaseClasses.Types;
using LotusCore.Utils;

namespace LotusCore.BaseClasses.World;

public class Entity
{
    //Metadata
    public Byte _EntityBitMaskValues = 0x00;
    public int _AirTicks = 300;
    public NBT? _CustomName;
    public bool _IsCustomNameVisible = false;
    public bool _IsSilent = false;
    public bool _HasNoGravity = false;
    public EntityPose _Pose = EntityPose.STANDING;
    public int _TicksFrozenInPowderSnow
    {
        get { return _TicksFrozenInPowderSnow; }
        set { Math.Clamp(value, 0, 140); }
    }

    //Entity Data
    public int _EntityID;
    public MinecraftUUID _EntityUUID;
    public Position _Position;
    public Angle _Pitch;
    public Angle _Yaw;
    public Angle _HeadPitch;
    public Angle _HeadYaw;
    public Velocity _Velocity;
    public int _Data;
}

public enum EntityBitMask
{
    IsOnFire = 0x01,
    IsPressingSneakKey = 0x02,
    IsSprinting = 0x08,
    IsSwimming = 0x10,
    IsInvisible = 0x20,
    HasGlowingEffect = 0x40,
    IsFlyingWithElytra = 0x80,
}

public enum EntityPose
{
    STANDING,
    FALL_FLYING,
    SLEEPING,
    SWIMMING,
    SPIN_ATTACK,
    SNEAKING,
    LONG_JUMPING,
    DYING,
    CROAKING,
    USING_TONGUE,
    SITTING,
    ROARING,
    SNIFFING,
    EMERGING,
    DIGGING,
    SLIDING,
    SHOOTING,
    INHALING,
}
