namespace Core_Engine.BaseClasses.PluginChannels.DataTypes;

struct ChunkSelectionPosition
{
    public int x,
        y,
        z;

    public ChunkSelectionPosition(long BigEndianPositionLong)
    {
        x = (int)(BigEndianPositionLong >> 42);
        z = (int)((BigEndianPositionLong >> 20) & 0x3FFFFF);
        y = (int)(BigEndianPositionLong & 0xFFFFF);
    }

    public long GetBits()
    {
        return ((x & 0x3FFFFF) << 42) | ((z & 0x3FFFFF) << 20) | (y & 0xFFFFF);
    }
}
