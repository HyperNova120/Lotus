namespace LotusCore.EngineEventArgs
{
    public class EngineEventArgs : EventArgs
    {
        object[] _Args;

        public EngineEventArgs(object[] args)
        {
            this._Args = args;
        }

        public object[] Args
        {
            get { return _Args; }
        }
    }
}
