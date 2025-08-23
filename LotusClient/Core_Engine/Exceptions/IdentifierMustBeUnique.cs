namespace LotusCore.Exceptions
{
    public class IdentifierMustBeUniqueException : Exception
    {
        public IdentifierMustBeUniqueException() { }

        public IdentifierMustBeUniqueException(string msg)
            : base(msg) { }

        public IdentifierMustBeUniqueException(string msg, Exception innerException)
            : base(msg, innerException) { }
    }
}
