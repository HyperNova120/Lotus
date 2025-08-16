using Core_Engine.BaseClasses.Types;

namespace Core_Engine.Modules.Networking.Packets.ClientBound.Configuration;

public class ConfigTransferPacket
{
    public string host = "";
    public int port;

    /// <summary>
    /// populates packet based on data
    /// </summary>
    /// <param name="data"></param>
    /// <returns>number of bytes
    public int DecodeFromBytes(byte[] data)
    {
        (host, int numBytes) = StringN.DecodeBytes(data);
        (port, int portNumBytes) = VarInt_VarLong.DecodeVarInt(data[numBytes..]);

        return portNumBytes + portNumBytes;
    }
}
