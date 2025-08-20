namespace Core_Engine.BaseClasses;

public class MinecraftUUID
{
    public UInt128 _UUID;

    public int DecodeBytes(byte[] inputBytes)
    {
        ulong msb = BitConverter.ToUInt64(inputBytes[..8].Reverse().ToArray());
        ulong lsb = BitConverter.ToUInt64(inputBytes[8..16].Reverse().ToArray());
        _UUID = msb;
        _UUID <<= 64;
        _UUID |= lsb;
        return 16;
    }
}
