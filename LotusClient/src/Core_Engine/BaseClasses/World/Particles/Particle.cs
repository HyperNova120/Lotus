namespace LotusCore.BaseClasses.World;

public class Particle
{
    public int _ID;

    public Identifier _ParticleName;

    public Particle(Identifier pName, int id)
    {
        _ParticleName = pName;
        _ID = id;
    }
}
