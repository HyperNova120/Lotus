namespace LotusCore.Interfaces;

public interface INetworkData<T>
{
    public static abstract byte[] GetBytes(T data);

    public static abstract T DecodeBytes(byte[] data, ref int offset);
}

public interface INetworkData<T, T2>
{
    public static abstract byte[] GetBytes(T2 data);

    public static abstract T DecodeBytes(byte[] data, ref int offset);
}
