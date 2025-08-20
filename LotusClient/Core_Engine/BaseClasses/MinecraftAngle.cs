namespace Core_Engine.BaseClasses;

public static class MinecraftAngle
{
    private static readonly float AngleToFloat = (256.0f / 360.0f);

    public static (float angle, int numBytesRead) DecodeBytes(byte[] inputBytes)
    {
        byte networkAngle = inputBytes[0];
        float tmp = BitConverter.ToSingle([0x00, 0x00, 0x00, networkAngle], 0);
        return (tmp / AngleToFloat, 1);
    }
}
