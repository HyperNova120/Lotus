using Core_Engine.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Core_Engine.Utils.NBT.BaseClasses;

public abstract class TAG_Base
{
    public int Type_ID = 0;
    public string Name = "";

    public bool isInListTag = false;

    public abstract byte[] ProcessBytes(byte[] inputBytes);

    public abstract string ToString(int tabSpace = 0);

    protected byte[] ProcessIDAndNameBytes(byte[] inputBytes)
    {
        if (isInListTag)
        {
            return inputBytes;
        }
        int packetTypeID = inputBytes[0];
        if (packetTypeID != this.Type_ID)
        {
            throw new IncorrectNBTTypeException(
                $"Packet ID from bytes does not match required packet id: Expected {Type_ID} Received {packetTypeID}"
            );
        }
        int nameLength = 0;
        nameLength = inputBytes[1];
        nameLength <<= 8;
        nameLength |= inputBytes[2];
        for (int i = 0; i < nameLength; i++)
        {
            this.Name += (char)inputBytes[3 + i];
        }
        return inputBytes[(3 + nameLength)..];
    }
}
