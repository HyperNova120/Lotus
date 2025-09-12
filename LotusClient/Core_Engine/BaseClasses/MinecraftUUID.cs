namespace LotusCore.BaseClasses;

public class MinecraftUUID
{
    public UInt128 _UUID;

    public void DecodeBytes(byte[] inputBytes, ref int offset)
    {
        ulong msb = BitConverter.ToUInt64(inputBytes[offset..(offset + 8)].Reverse().ToArray());
        ulong lsb = BitConverter.ToUInt64(
            inputBytes[(offset + 8)..(offset + 16)].Reverse().ToArray()
        );
        _UUID = msb;
        _UUID <<= 64;
        _UUID |= lsb;
        offset += 16;
    }
}
