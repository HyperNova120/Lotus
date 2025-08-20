using Core_Engine.BaseClasses;
using Core_Engine.BaseClasses.Types;

namespace Core_Engine.Modules.Networking.Packets.ClientBound.Play;

public class SpawnEntityPacket
{
    int _EntityID;

    MinecraftUUID _EntityUUID;

    int _Type;

    double _X;

    double _Y;

    double _Z;

    float _Pitch;

    float _Yaw;

    float _HeadYaw;

    int _Data;

    short _VelocityX;

    short _VelocityY;

    short _VelocityZ;

    public int DecodeBytes(byte[] inputBytes)
    {
        (_EntityID, int offset) = VarInt_VarLong.DecodeVarInt(inputBytes);
        _EntityUUID = new();
        offset += _EntityUUID.DecodeBytes(inputBytes[offset..]);
    }
}
