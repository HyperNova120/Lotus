namespace LotusCore.BaseClasses.World.Entities;

public class Mob : LivingEntity
{
    public byte _mobData = 0;

    public enum MobDataFlags
    {
        NO_AI = 0x1,
        IS_LEFT_HANDED = 0x2,
        IS_AGGRESSIVE = 0x4,
    }
}
