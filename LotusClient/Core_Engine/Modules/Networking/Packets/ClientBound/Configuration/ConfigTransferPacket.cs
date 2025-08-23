using LotusCore.BaseClasses.Types;

namespace LotusCore.Modules.Networking.Packets.ClientBound.Configuration;

public class ConfigTransferPacket
{
    public string _Host = "";
    public int _Port;

    /// <summary>
    /// populates packet based on data
    /// </summary>
    /// <param name="data"></param>
    /// <returns>number of bytes
    public int DecodeFromBytes(byte[] data)
    {
        (_Host, int numBytes) = StringN.DecodeBytes(data);
        (_Port, int portNumBytes) = VarInt_VarLong.DecodeVarInt(data[numBytes..]);

        return portNumBytes + portNumBytes;
    }
}
