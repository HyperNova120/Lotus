using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;

namespace LotusCore.Modules.Networking.Packets.ClientBound.Play;

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
        int offset = 0;
        _EntityID = VarInt_VarLong.DecodeVarInt(inputBytes, ref offset);

        _EntityUUID = new();
        _EntityUUID.DecodeBytes(inputBytes, ref offset);

        _Type = VarInt_VarLong.DecodeVarInt(inputBytes, ref offset);

        _X = NetworkDouble.DecodeBytes(inputBytes, ref offset);

        _Y = NetworkDouble.DecodeBytes(inputBytes, ref offset);

        _Z = NetworkDouble.DecodeBytes(inputBytes, ref offset);

        _Pitch = MinecraftAngle.DecodeBytes(inputBytes, ref offset);

        _Yaw = MinecraftAngle.DecodeBytes(inputBytes, ref offset);

        _HeadYaw = MinecraftAngle.DecodeBytes(inputBytes, ref offset);

        _Data = VarInt_VarLong.DecodeVarInt(inputBytes, ref offset);

        _VelocityX = NetworkShort.DecodeBytes(inputBytes, ref offset);
        _VelocityY = NetworkShort.DecodeBytes(inputBytes, ref offset);
        _VelocityZ = NetworkShort.DecodeBytes(inputBytes, ref offset);

        return offset;
    }
}
