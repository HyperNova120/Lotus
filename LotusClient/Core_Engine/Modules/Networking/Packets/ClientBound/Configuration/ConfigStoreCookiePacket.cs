using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;
using LotusCore.Modules.GameStateHandlerModule.BaseClasses;

namespace LotusCore.Modules.Networking.Packets.ClientBound.Configuration;

public class StoreCookiePacket
{
    public ServerCookie? _ServerCookie;

    /// <summary>
    /// populates packet based on data
    /// </summary>
    /// <param name="data"></param>
    /// <returns>number of bytes read</returns>
    public int DecodeFromBytes(byte[] data)
    {
        _ServerCookie = new();
        _ServerCookie._Key = new Identifier();
        int offset = 0;
        _ServerCookie._Key.GetFromBytes(data, ref offset);
        byte[] bytesOfArray = PrefixedArray.DecodeBytes(data, ref offset);

        _ServerCookie._Payload = bytesOfArray;

        return offset;
    }
}
