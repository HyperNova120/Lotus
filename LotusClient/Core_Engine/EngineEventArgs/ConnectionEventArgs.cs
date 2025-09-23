using System.Net;
using LotusCore.Modules.Networking.Packets;

namespace LotusCore.EngineEventArgs
{
    public class ConnectionEventArgs : IEngineEventArgs
    {
        public Guid _remoteHostID { get; private set; }

        public ConnectionEventArgs(Guid remoteHostID)
        {
            this._remoteHostID = remoteHostID;
        }
    }
}
