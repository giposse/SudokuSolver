namespace SudokuSolver.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    public class InvalidBoardSizeException : Exception
    {
        public InvalidBoardSizeException() : base() { }
        public InvalidBoardSizeException(string message) : base(message) { }
        public InvalidBoardSizeException(string message, Exception inner) : base(message, inner) { }
        public InvalidBoardSizeException(SerializationInfo info, StreamingContext context) : base( info, context) { }
    }
}
