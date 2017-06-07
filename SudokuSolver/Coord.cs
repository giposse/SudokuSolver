using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver
{
    public static class Coord
    {
        private static string[] Names = new string[] { "row", "column", "box" };
        public const int ROW = 0;
        public const int ROWS = 0;
        public const int BYROWS = 0;
        public const int COL = 1;
        public const int COLUMNS = 1;
        public const int BYCOLS = 1;
        public const int SECTION = 2;
        public const int SECTIONS = 2;
        public const int BY_SECTIONS = 2;
        public const int COORD_COUNT = 3;

        public static string Name(int index)
        {
            return Names[index];
        }
    }
}
