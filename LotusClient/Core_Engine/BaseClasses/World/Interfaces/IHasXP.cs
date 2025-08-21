namespace Core_Engine.BaseClasses.World.Interfaces;

public interface IHasXP
{
    public float GetExpereinceBarValue();
    public void SetExpereinceBarValue();
    public int GetXPLevel();
    public void SetXPLevel();
    public int GetTotalXP();
    public void SetTotalXP();

    public int GetXPRequiredForNextLevel()
    {
        int currentLevel = GetXPLevel();
        if (currentLevel >= 0 && currentLevel <= 15)
        {
            return (2 * currentLevel) + 7;
        }
        else if (currentLevel >= 16 && currentLevel <= 30)
        {
            return (5 * currentLevel) - 38;
        }
        else
        {
            return (9 * currentLevel) - 156;
        }
    }
}
