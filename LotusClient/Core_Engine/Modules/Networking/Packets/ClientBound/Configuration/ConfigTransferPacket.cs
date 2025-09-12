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
        int offset = 0;
        _Host = StringN.DecodeBytes(data, ref offset);
        _Port = VarInt_VarLong.DecodeVarInt(data, ref offset);

        return offset;
    }
}
