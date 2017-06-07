using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver.GameParts.SortHelpers
{
    internal class CellSorter : IComparer<Cell>
    {
        private bool SortByRows { get; set; }

        public CellSorter(bool byRows)
        {
            SortByRows = byRows;
        }

        public int Compare(Cell cell1, Cell cell2)
        {
            int returnValue = SortByRows ? cell1.RowIndex - cell2.RowIndex : cell1.ColIndex - cell2.ColIndex;
            if (returnValue == 0) 
                returnValue = SortByRows ? cell1.ColIndex = cell2.ColIndex : cell1.RowIndex - cell2.RowIndex;

            return returnValue;
        }
    }
}
