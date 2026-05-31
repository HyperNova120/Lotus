namespace LotusCore.Exceptions
{
    public class IncorrectNBTTypeException : Exception
    {
        public IncorrectNBTTypeException() { }

        public IncorrectNBTTypeException(string msg)
            : base(msg) { }

        public IncorrectNBTTypeException(string msg, Exception innerException)
            : base(msg, innerException) { }
    }
}
