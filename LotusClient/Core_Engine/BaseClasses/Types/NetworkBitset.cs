using System.Collections;
using LotusCore.Interfaces;

namespace LotusCore.BaseClasses.Types;

public class NetworkBitset : INetworkData<BitArray>
{
    public static BitArray DecodeBytes(byte[] data, ref int offset)
    {
        int size = VarInt_VarLong.DecodeVarInt(data, ref offset);
        BitArray returner = new(sizeof(long) * 8 * size);
        for (int i = 0; i < size; i++)
        {
            long curValue = NetworkLong.DecodeBytes(data, ref offset);
            for (int bit = 0; bit < (sizeof(long) * 8); bit++)
            {
                returner[i * (sizeof(long) * 8) + bit] = ((curValue >> bit) & 0x01) == 1;
            }
        }
        return returner;
    }

    public static byte[] GetBytes(BitArray data)
    {
        throw new NotImplementedException();
    }
}
