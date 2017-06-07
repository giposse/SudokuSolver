namespace SudokuSolver.GameParts
{
	using System.Collections.Generic;

	public class ALSMatch
    {
        public AlmostLockedSet Als1 { get; set; }
        public AlmostLockedSet Als2 { get; set; }
        public List<Cell> AffectedCells { get; set; }
        public int commonPencilmark;
        public int commonRestrictedPencilmark;
    }
}
