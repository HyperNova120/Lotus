using System.Net.NetworkInformation;
using LotusCore.Interfaces;

namespace LotusCore.BaseClasses.Types;

public class NetworkIDOrX<T, T2> where T2 : INetworkData<T>
{
    public static byte[] GetBytes(int data)
    {
        return [.. BitConverter.GetBytes(data).Reverse()];
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="value"></param>
    /// <returns>0 if X is inline, otherwise registry ID + 1</returns>
    public static int DecodeBytes(byte[] data, ref int offset, out T? value)
    {
        int ID = VarInt_VarLong.DecodeVarInt(data, ref offset);
        if (ID != 0)
        {
            value = default(T);
            return ID;
        }

        value = T2.DecodeBytes(data, ref offset);
        return 0;
    }
}
