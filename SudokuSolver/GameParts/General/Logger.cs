namespace SudokuSolver.GameParts.General
{
	using System.Diagnostics;
	using System.Text;

	public class Logger
    {
        private StringBuilder logData;

        public Logger() { logData = new StringBuilder(); }

        [Conditional("DEBUG")]
        public void Clear() { logData.Clear(); }


        [Conditional("DEBUG")]
        public void WriteLine(string formatString, params object[] args)
        {
            logData.AppendFormat(formatString + "\r\n", args);
        }
    }
}
