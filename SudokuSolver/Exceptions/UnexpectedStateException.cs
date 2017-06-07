namespace SudokuSolver.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    public class UnexpectedStateException : Exception
    {
        public UnexpectedStateException() : base() { }
        public UnexpectedStateException(string message) : base(message) { }
        public UnexpectedStateException(string message, Exception inner) : base(message, inner) { }
        public UnexpectedStateException(SerializationInfo serInfo, StreamingContext context) : base(serInfo, context) { }
    }
}
