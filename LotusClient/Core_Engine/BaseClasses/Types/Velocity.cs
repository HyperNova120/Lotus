namespace LotusCore.BaseClasses.Types;

public class Velocity
{
    public float _PerTickMulitplier = (1 / 8000);

    public short _XVel;
    public short _YVel;
    public short _ZVel;

    public float GetXMoveForSingleTick()
    {
        return _PerTickMulitplier * _XVel;
    }

    public float GetYMoveForSingleTick()
    {
        return _PerTickMulitplier * _YVel;
    }

    public float GetZMoveForSingleTick()
    {
        return _PerTickMulitplier * _ZVel;
    }
}
