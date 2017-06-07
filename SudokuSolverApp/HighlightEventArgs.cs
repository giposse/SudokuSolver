namespace SudokuSolverApp
{
    using System;
    using System.Drawing;
    
    public class HighlightEventArgs : EventArgs
    {
        public bool Active { get; set; } // true to turn on highlighting
        public Color? BackgroundColor;
        public Color? ForegroundColor;
    }
}
