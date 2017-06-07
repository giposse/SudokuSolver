namespace SudokuSolver.GameParts.General
{
    using System.Collections.Generic;

    public class CellSorter : IComparer<Cell> , IEqualityComparer<Cell>
    {
        private int coord;

        public CellSorter(int coord)
        {
            this.coord = coord;
        }

        public int Compare(Cell cell1, Cell cell2)
        {
            int returnValue = cell1.GetCoordinate(coord) - cell2.GetCoordinate(coord);
            if (returnValue == 0)
            {
                int secCoord = 1 - coord;
                returnValue = cell1.GetCoordinate(secCoord) - cell2.GetCoordinate(secCoord);
            }

            return returnValue;
        }

        #region IEquality<Cell> implementation
        public bool Equals(Cell c1, Cell c2)
        {
            bool returnValue = new CellSorter(Coord.BYROWS).Compare(c1, c2) == 0;
            return returnValue;
        }

        public int GetHashCode(Cell cell)
        {
            int hash = 17 + 29 * (cell.ColIndex + cell.RowIndex + cell.GroupIndex +
                cell.PMSignature + cell.Value + 5/* + 1 for each addend */);

            return hash;
        }
        #endregion
    }
}
