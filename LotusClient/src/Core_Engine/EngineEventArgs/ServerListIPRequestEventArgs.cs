using System.Net;
using System.Net.Sockets;

namespace LotusCore.EngineEventArgs
{
    public class ServerListIPRequestEventArgs : IEngineEventArgs
    {
        public string _serverName;

        public ServerListIPRequestEventArgs(string name)
        {
            _serverName = name;
        }
    }
}
