using Core_Engine.BaseClasses;

namespace Core_Engine.Modules.GameStateHandler.BaseClasses
{
    public class ServerCookie
    {
        public Identifier? Key;
        public byte[]? Payload;

        public ServerCookie() { }

        public ServerCookie(Identifier Identifier, byte[] payload)
        {
            this.Key = Identifier;
            this.Payload = payload;
        }
    }
}
