namespace Core_Engine.EngineEventArgs
{
    public class EngineEventArgs : EventArgs
    {
        object[] args;

        public EngineEventArgs(object[] args)
        {
            this.args = args;
        }

        public object[] Args
        {
            get { return args; }
        }
    }
}
