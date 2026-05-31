using LotusCore.BaseClasses.World.Entities;

namespace LotusCore.BaseClasses.World.Interfaces;

public interface IAttackable
{
    public LivingEntity? GetLastAttacker();

    public void SetLastAttacker();
}
