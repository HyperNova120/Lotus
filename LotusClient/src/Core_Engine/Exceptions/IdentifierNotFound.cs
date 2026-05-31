namespace LotusCore.Exceptions
{
    public class IdentifierNotFoundException : Exception
    {
        public IdentifierNotFoundException() { }

        public IdentifierNotFoundException(string msg)
            : base(msg) { }

        public IdentifierNotFoundException(string msg, Exception innerException)
            : base(msg, innerException) { }
    }
}
