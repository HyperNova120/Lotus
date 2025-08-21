namespace Core_Engine.BaseClasses.Entity;

public interface IAttackable
{
    public LivingEntity? GetLastAttacker();

    public void SetLastAttacker();
}
