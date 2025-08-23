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
}
