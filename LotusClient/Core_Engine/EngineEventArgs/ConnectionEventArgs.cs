using System.Net;
using Core_Engine.Modules.Networking.Packets;

namespace Core_Engine.EngineEventArgs
{
    public class ConnectionEventArgs : EventArgs
    {
        public IPAddress _RemoteHost { get; private set; }

        public ConnectionEventArgs(IPAddress remoteHost)
        {
            this._RemoteHost = remoteHost;
        }
    }
}
