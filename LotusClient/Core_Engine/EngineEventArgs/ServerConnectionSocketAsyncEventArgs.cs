using System.Net;
using System.Net.Sockets;

namespace LotusCore.EngineEventArgs
{
    public class ServerConnectionSocketAsyncEventArgs : SocketAsyncEventArgs
    {
        /// <summary>
        /// server connection linked to this event
        /// </summary>
        public IPAddress _RemoteHost { get; private set; }

        public ServerConnectionSocketAsyncEventArgs(IPAddress remoteHost)
            : base()
        {
            this._RemoteHost = remoteHost;
        }
    }
}
