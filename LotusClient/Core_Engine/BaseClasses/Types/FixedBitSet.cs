using System.Reflection.Metadata.Ecma335;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace LotusCore.BaseClasses.Types;

/// <summary>
/// index 0 = msb, index _maxSize-1 = lsb
/// </summary>
public class FixedBitSet
{
    private int _maxSize;

    private byte[] _data;

    public FixedBitSet(int maxSize)
    {
        _maxSize = maxSize;
        _data = new byte[(maxSize + 7) / 8];
    }

    public FixedBitSet(FixedBitSet other)
    {
        _maxSize = other._maxSize;
        _data = new byte[other._data.Length];
        Array.Copy(other._data, _data, other._data.Length);
    }

    public FixedBitSet(byte[] data, ref int offset, int maxSize)
    {
        _maxSize = maxSize;
        int len = (maxSize + 7) / 8;
        _data = new byte[len];
        Array.Copy(data, offset, _data, 0, len);
        offset += len;
    }

    public bool this[int index]
    {
        get => Get(index);
        set => Set(index, value);
    }

    /* private bool Get(int index)
    {
        if (index < 0 || index >= _maxSize)
        {
            throw new IndexOutOfRangeException(nameof(index));
        }

        int byteIndex = _data.Length - (index / 8) - 1;
        int bitIndex = 7 - (index % 8);

        return (_data[byteIndex] & (0x01 << bitIndex)) != 0;
    }

    private bool Set(int index, bool value)
    {
        if (index < 0 || index >= _maxSize)
            throw new IndexOutOfRangeException(nameof(index));

        int byteIndex = _data.Length - (index / 8) - 1;
        int bitIndex = 7 - (index % 8);
        byte mask = (byte)(1 << bitIndex);

        if (value)
        {
            _data[byteIndex] |= mask;
        }
        else
        {
            _data[byteIndex] &= (byte)~mask;
        }

        return true;
    } */

    private void CalculateIndexValues(int index, out int byteIndex, out int bitIndex)
    {
        byteIndex = index / 8;
        bitIndex = index % 8;
    }

    private bool Get(int index)
    {
        if (index < 0 || index >= _maxSize)
            throw new IndexOutOfRangeException(nameof(index));
        CalculateIndexValues(index, out int byteIndex, out int bitIndex);
        return (_data[byteIndex] & (1 << bitIndex)) != 0;
    }

    private bool Set(int index, bool value)
    {
        if (index < 0 || index >= _maxSize)
            throw new IndexOutOfRangeException(nameof(index));

        CalculateIndexValues(index, out int byteIndex, out int bitIndex);
        byte mask = (byte)(1 << bitIndex);

        if (value)
            _data[byteIndex] |= mask;
        else
            _data[byteIndex] &= (byte)~mask;

        return true;
    }

    public byte[] GetBytes()
    {
        byte[] returner = new byte[_data.Length];
        foreach (byte b in _data)
        {
            // Convert to binary string and pad with leading zeros
            string binary = Convert.ToString(b, 2).PadLeft(8, '0');
            Console.Write(binary + " ");
        }
        Console.WriteLine();

        Array.Copy(_data, returner, _data.Length);
        return returner;
    }
}
