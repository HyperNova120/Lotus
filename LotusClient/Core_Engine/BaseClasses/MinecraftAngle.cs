namespace LotusCore.BaseClasses;

public static class MinecraftAngle
{
    private static readonly float AngleToFloat = (256.0f / 360.0f);

    public static float DecodeBytes(byte[] inputBytes, ref int offset)
    {
        byte networkAngle = inputBytes[offset++];
        float tmp = BitConverter.ToSingle([0x00, 0x00, 0x00, networkAngle], 0);
        return tmp / AngleToFloat;
    }
}
