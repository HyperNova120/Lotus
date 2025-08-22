using System.Text;
using Core_Engine.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Core_Engine.Utils.NBTInternals.BaseClasses;

public abstract class TAG_Base
{
    public enum TagTypeID
    {
        TAG_END,
        TAG_BYTE,
        TAG_SHORT,
        TAG_INT,
        TAG_LONG,
        TAG_FLOAT,
        TAG_DOUBLE,
        TAG_BYTE_ARRAY,
        TAG_STRING,
        TAG_LIST,
        TAG_COMPOUND,
        TAG_INT_ARRAY,
        TAG_LONG_ARRAY,
    };

    public int _Type_ID = 0;
    public string? _Name = "";

    public bool _IsInListTag = false;

    public abstract int ProcessBytes(byte[] inputBytes);

    public abstract string ToString(int tabSpace = 0);

    protected int ProcessIDAndNameBytes(byte[] inputBytes)
    {
        if (_IsInListTag)
        {
            return 0;
        }
        int packetTypeID = inputBytes[0];
        if (packetTypeID != this._Type_ID)
        {
            throw new IncorrectNBTTypeException(
                $"Packet ID from bytes does not match required packet id: Expected {_Type_ID} Received {packetTypeID}"
            );
        }
        int nameLength;
        nameLength = inputBytes[1];
        nameLength <<= 8;
        nameLength |= inputBytes[2];
        for (int i = 0; i < nameLength; i++)
        {
            this._Name += (char)inputBytes[3 + i];
        }
        return 3 + nameLength;
    }

    public abstract byte[] GetBytes();

    protected byte[] GetIDAndNamesBytes()
    {
        if (_IsInListTag)
        {
            return [];
        }

        byte idByte = (byte)_Type_ID;
        if (this._Name == null)
        {
            return [idByte];
        }

        //add length of name
        byte[] nameBytes = Encoding.UTF8.GetBytes(_Name);
        return
        [
            idByte,
            (byte)((nameBytes.Length & 0xFF00) >> 8),
            (byte)(nameBytes.Length & 0xFF),
            .. nameBytes,
        ];
    }

    public abstract TAG_Base Clone();
}
