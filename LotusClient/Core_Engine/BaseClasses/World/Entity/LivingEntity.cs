using System.Security;
using LotusCore.BaseClasses.World.Interfaces;

namespace LotusCore.BaseClasses.World;

public class LivingEntity : Entity, IAttackable
{
    public LivingEntity? GetLastAttacker()
    {
        throw new NotImplementedException();
    }

    public void SetLastAttacker()
    {
        throw new NotImplementedException();
    }

    //LivingEntity Metadata

    byte _HandStands = 0x00;

    float _Health = 1.0f;

    //Particles _PotionEffectColor;

    bool _IsPotionEffectAmbient = false;

    int _NumberArrowsInEntity = 0;

    int _NumberBeeStingersInEntity = 0;

    Position? _LocationOfCurrentSleepingBed;
}

public enum LivingEntityHandStates
{
    IsHandActive = 0x01,
    ActiveHand = 0x02, //0 = main, 1 = offhand
    IsInRiptideSpinAttack = 0x04,
}
