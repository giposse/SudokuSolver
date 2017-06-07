namespace SudokuSolver.GameParts
{
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Class used only for testing purposes
	/// </summary>
	public class AlmostLockedSetEqualityComparer : IEqualityComparer<AlmostLockedSet>
    {
        public bool Equals(AlmostLockedSet x, AlmostLockedSet y)
        {
            int cellCount = x.Cells.Count;
            bool returnValue = cellCount == y.Cells.Count &&
                x.Cells.FirstOrDefault(cell => !y.Cells.Contains(cell)) == null;

            return returnValue;
        }

        public int GetHashCode(AlmostLockedSet als)
        {
            int hash = 17;
            als.Cells.ForEach(cell => hash += 29 * cell.GetHashCode());
            return hash;
        }
    }
}
