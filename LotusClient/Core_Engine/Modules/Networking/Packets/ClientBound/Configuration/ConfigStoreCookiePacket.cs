using LotusCore.BaseClasses;
using LotusCore.BaseClasses.Types;
using LotusCore.Modules.GameStateHandler.BaseClasses;

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
        int numBytesRead = _ServerCookie._Key.GetFromBytes(data);
        (byte[] bytesOfArray, int numberBytesRead) = PrefixedArray.DecodeBytes(
            data[numBytesRead..]
        );

        _ServerCookie._Payload = bytesOfArray;

        return numBytesRead + numberBytesRead;
    }
}
