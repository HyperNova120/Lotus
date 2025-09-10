namespace LotusCore.BaseClasses;

public class Position
{
    public int _X,
        _Y,
        _Z;

    public Position()
    {
        _X = 0;
        _Y = 0;
        _Z = 0;
    }

    public long GetAsBigEndianLongValue()
    {
        return ((_X & 0x3FFFFFF) << 38) | ((_Z & 0x3FFFFFF) << 12) | (_Y & 0xFFF);
    }

    public void SetFromBigEndianLong(long BigEndianPositionLong)
    {
        _X = (int)(BigEndianPositionLong >> 38);
        _Z = (int)((BigEndianPositionLong >> 12) & 0x3FFFFFF);
        _Y = (int)(BigEndianPositionLong & 0xFFF);
    }
}
