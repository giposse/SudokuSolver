namespace SudokuSolver
{
    using System;
    using System.Runtime.Serialization;

    public class InvalidMoveException : Exception
    {
        public InvalidMoveException() : base() { }
        public InvalidMoveException(string message) : base(message) { }
        public InvalidMoveException(string message, Exception innerException) : base(message, innerException) { }
        public InvalidMoveException(SerializationInfo serializationInfo, StreamingContext context) : base(serializationInfo, context) { }
    }
}
