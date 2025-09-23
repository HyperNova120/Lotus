using System.Net;
using System.Net.Sockets;

namespace LotusCore.EngineEventArgs
{
    public class ServerConnectionSocketAsyncEventArgs : SocketAsyncEventArgs, IEngineEventArgs
    {
        /// <summary>
        /// server connection linked to this event
        /// </summary>
        public Guid _remoteHostID { get; private set; }

        public ServerConnectionSocketAsyncEventArgs(Guid remoteHostID)
            : base()
        {
            this._remoteHostID = remoteHostID;
        }
    }
}
