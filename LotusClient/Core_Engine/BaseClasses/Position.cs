namespace Core_Engine.BaseClasses;

struct Position
{
    public int x,
        y,
        z;

    public Position(long BigEndianPositionLong)
    {
        x = (int)(BigEndianPositionLong >> 38);
        z = (int)((BigEndianPositionLong >> 12) & 0x3FFFFFF);
        y = (int)(BigEndianPositionLong & 0xFFF);
    }

    public long GetBits()
    {
        return ((x & 0x3FFFFFF) << 38) | ((z & 0x3FFFFFF) << 12) | (y & 0xFFF);
    }
}
