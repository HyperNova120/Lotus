using LotusCore.Exceptions;
using LotusCore.Utils.NBTInternals.BaseClasses;

namespace LotusCore.Utils.NBTInternals.Tags;

public class TAG_End : TAG_Base
{
    public TAG_End()
    {
        _Type_ID = 0;
    }

    public override TAG_Base Clone()
    {
        return new TAG_End();
    }

    public override byte[] GetBytes()
    {
        throw new NotImplementedException();
    }

    public override int ProcessBytes(byte[] inputBytes)
    {
        int packetTypeID = inputBytes[0];
        if (packetTypeID != this._Type_ID)
        {
            throw new IncorrectNBTTypeException(
                "Packet ID from bytes does not match required packet id"
            );
        }
        return 1;
    }

    public override string ToString(int tabSpace = 0)
    {
        return "";
    }
}
