using LotusCore.BaseClasses;

namespace LotusCore.Modules.GameStateHandlerModule.BaseClasses
{
    public class ServerCookie
    {
        public Identifier? _Key;
        public byte[]? _Payload;

        public ServerCookie() { }

        public ServerCookie(Identifier Identifier, byte[] payload)
        {
            this._Key = Identifier;
            this._Payload = payload;
        }
    }
}
