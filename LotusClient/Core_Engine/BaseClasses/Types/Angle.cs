namespace LotusCore.BaseClasses.Types;

public class Angle
{
    private readonly float AngleMod = (360.0f / 256.0f);
    public float _Angle;

    public void GetRealAngle(byte angle)
    {
        _Angle = angle * AngleMod;
    }

    public byte GetAngleByte()
    {
        return (byte)((_Angle % 360.0f) / AngleMod);
    }
}
