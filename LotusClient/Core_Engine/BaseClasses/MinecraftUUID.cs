namespace LotusCore.BaseClasses;

public class MinecraftUUID
{
    public UInt128 _UUID;

    public MinecraftUUID() { }

    public MinecraftUUID(string uuidString)
    {
        uuidString = uuidString.Replace("-", "");
        if (uuidString.Length != 32)
            throw new ArgumentException("UUID string must be 32 characters long");

        byte[] bytes = new byte[16];
        for (int i = 0; i < 16; i++)
        {
            string hexPair = uuidString.Substring(i * 2, 2);
            bytes[i] = Convert.ToByte(hexPair, 16);
        }

        // Convert to UInt128 (big-endian)
        ulong msb = BitConverter.ToUInt64(bytes[0..8].Reverse().ToArray());
        ulong lsb = BitConverter.ToUInt64(bytes[8..16].Reverse().ToArray());

        _UUID = msb;
        _UUID <<= 64;
        _UUID |= lsb;
    }

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

    public byte[] GetBytes()
    {
        byte[] bytes = new byte[16];

        ulong msb = (ulong)(_UUID >> 64);
        ulong lsb = (ulong)(_UUID & 0xFFFFFFFFFFFFFFFF);

        byte[] msbBytes = BitConverter.GetBytes(msb).Reverse().ToArray();
        byte[] lsbBytes = BitConverter.GetBytes(lsb).Reverse().ToArray();

        return [.. msbBytes, .. lsbBytes];
    }
}
