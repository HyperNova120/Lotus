using Core_Engine.Exceptions;
using Core_Engine.Utils.NBTInternals.BaseClasses;

namespace Core_Engine.Utils.NBTInternals.Tags;

public class TAG_End : TAG_Base
{
    public TAG_End()
    {
        Type_ID = 0;
    }

    public override byte[] GetBytes()
    {
        throw new NotImplementedException();
    }

    public override byte[] ProcessBytes(byte[] inputBytes)
    {
        int packetTypeID = inputBytes[0];
        if (packetTypeID != this.Type_ID)
        {
            throw new IncorrectNBTTypeException(
                "Packet ID from bytes does not match required packet id"
            );
        }
        return inputBytes[1..];
    }

    public override string ToString(int tabSpace = 0)
    {
        return "";
    }
}
