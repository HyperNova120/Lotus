namespace Core_Engine.BaseClasses.World.Interfaces;

public interface IAttackable
{
    public LivingEntity? GetLastAttacker();

    public void SetLastAttacker();
}
