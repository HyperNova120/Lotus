using System.Net;
using LotusCore.Modules.Networking.Packets;

namespace LotusCore.EngineEventArgs
{
    public class ConnectionEventArgs : IEngineEventArgs
    {
        public IPAddress _RemoteHost { get; private set; }

        public ConnectionEventArgs(IPAddress remoteHost)
        {
            this._RemoteHost = remoteHost;
        }
    }
}
