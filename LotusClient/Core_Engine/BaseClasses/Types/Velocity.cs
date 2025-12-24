namespace LotusCore.BaseClasses.Types;

public class Velocity
{
    public float _PerTickMulitplier = (1 / 8000);

    public double _XVel;
    public double _YVel;
    public double _ZVel;

    public float GetXMoveForSingleTick()
    {
        return (float)(_PerTickMulitplier * _XVel);
    }

    public float GetYMoveForSingleTick()
    {
        return (float)(_PerTickMulitplier * _YVel);
    }

    public float GetZMoveForSingleTick()
    {
        return (float)(_PerTickMulitplier * _ZVel);
    }
}
