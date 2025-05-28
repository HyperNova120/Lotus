using System.Net;
using System.Net.Sockets;

namespace Core_Engine.EngineEventArgs
{
    public class ServerConnectionSocketAsyncEventArgs : SocketAsyncEventArgs
    {
        /// <summary>
        /// server connection linked to this event
        /// </summary>
        public IPAddress remoteHost { get; private set; }

        public ServerConnectionSocketAsyncEventArgs(IPAddress remoteHost)
            : base()
        {
            this.remoteHost = remoteHost;
        }
    }
}
