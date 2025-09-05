using System.Net;
using LotusCore.BaseClasses;
using static LotusCore.Modules.Networking.Networking;

namespace LotusCore.EngineEventArgs;

public class PluginMessageReceivedEventArgs : IEngineEventArgs
{
    public IPAddress _RemoteHost { get; private set; }

    public ConnectionState _ConnectionState { get; private set; }

    public int? _MessageID { get; private set; }

    public Identifier _Channel { get; private set; }

    public byte[] _Data { get; private set; }

    public PluginMessageReceivedEventArgs(
        IPAddress remoteHost,
        ConnectionState connectionState,
        Identifier channel,
        byte[] data,
        int? messageID = null
    )
    {
        _RemoteHost = remoteHost;
        _ConnectionState = connectionState;
        _Channel = channel;
        _Data = data;
        _MessageID = messageID;
    }
}
