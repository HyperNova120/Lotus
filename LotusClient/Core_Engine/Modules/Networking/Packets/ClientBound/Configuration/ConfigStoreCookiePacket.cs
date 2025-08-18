using Core_Engine.BaseClasses;
using Core_Engine.BaseClasses.Types;
using Core_Engine.Modules.GameStateHandler.BaseClasses;

namespace Core_Engine.Modules.Networking.Packets.ClientBound.Configuration;

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
