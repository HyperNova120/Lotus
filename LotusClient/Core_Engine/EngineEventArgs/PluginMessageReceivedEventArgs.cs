using System.Net;
using Core_Engine.BaseClasses;
using static Core_Engine.Modules.Networking.Networking;

namespace Core_Engine.EngineEventArgs;

public class PluginMessageReceivedEventArgs : EventArgs
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
