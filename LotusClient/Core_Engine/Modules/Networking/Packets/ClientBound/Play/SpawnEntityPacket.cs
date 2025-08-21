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

        (_Type, int numBytesRead) = VarInt_VarLong.DecodeVarInt(inputBytes[offset..]);
        offset += numBytesRead;

        (_X, numBytesRead) = NetworkDouble.DecodeBytes(inputBytes[offset..]);
        offset += numBytesRead;

        (_Y, numBytesRead) = NetworkDouble.DecodeBytes(inputBytes[offset..]);
        offset += numBytesRead;

        (_Z, numBytesRead) = NetworkDouble.DecodeBytes(inputBytes[offset..]);
        offset += numBytesRead;

        (_Pitch, int numBytesRead5) = MinecraftAngle.DecodeBytes(inputBytes[offset..]);
        offset += numBytesRead;

        (_Yaw, numBytesRead) = MinecraftAngle.DecodeBytes(inputBytes[offset..]);
        offset += numBytesRead;

        (_HeadYaw, numBytesRead) = MinecraftAngle.DecodeBytes(inputBytes[offset..]);
        offset += numBytesRead;

        (_Data, numBytesRead) = VarInt_VarLong.DecodeVarInt(inputBytes[offset..]);
        offset += numBytesRead;

        (_VelocityX, numBytesRead) = NetworkShort.DecodeBytes(inputBytes[offset..]);
        offset += numBytesRead;
        (_VelocityY, numBytesRead) = NetworkShort.DecodeBytes(inputBytes[offset..]);
        offset += numBytesRead;
        (_VelocityZ, numBytesRead) = NetworkShort.DecodeBytes(inputBytes[offset..]);
        offset += numBytesRead;

        return offset;
    }
}
