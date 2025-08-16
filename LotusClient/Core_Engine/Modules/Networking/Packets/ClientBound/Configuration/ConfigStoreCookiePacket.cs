using Core_Engine.BaseClasses;
using Core_Engine.BaseClasses.Types;
using Core_Engine.Modules.GameStateHandler.BaseClasses;

namespace Core_Engine.Modules.Networking.Packets.ClientBound.Configuration;

public class StoreCookiePacket
{
    public ServerCookie? serverCookie;

    /// <summary>
    /// populates packet based on data
    /// </summary>
    /// <param name="data"></param>
    /// <returns>number of bytes read</returns>
    public int DecodeFromBytes(byte[] data)
    {
        serverCookie = new();
        serverCookie.Key = new Identifier();
        int numBytesRead = serverCookie.Key.GetFromBytes(data);
        (byte[] bytesOfArray, int numberBytesRead) = PrefixedArray.DecodeBytes(
            data[numBytesRead..]
        );

        serverCookie.Payload = bytesOfArray;

        return numBytesRead + numberBytesRead;
    }
}
