namespace SudokuSolver.GameParts
{
	using System;

	internal class PencilMarkInfo : IComparable<PencilMarkInfo>
    {
        /// <summary>
        /// How many pencil marks are valid
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Integer, where the set bits are the valid pencil marks for the cell
        /// </summary>
        public int Signature { get; set; }

        /// <summary>
        /// Cell for which this information relates to
        /// </summary>
        public Cell Cell { get; set; }

        public int CompareTo(PencilMarkInfo pmi2)
        {
            int returnValue = this.Count - pmi2.Count;
            if (returnValue == 0) returnValue = this.Signature - pmi2.Signature;
            return returnValue;
        }
    }
}
