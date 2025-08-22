using Core_Engine.BaseClasses.World.Interfaces;

namespace Core_Engine.BaseClasses.World;

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
