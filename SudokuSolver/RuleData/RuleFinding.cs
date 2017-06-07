namespace SudokuSolver.RuleData
{
    using System.Collections.Generic;
    using GameParts;

    public class RuleFinding
    {
        public Cell Cell { get; set; }
        public CellRole CellRole { get; set; }
        public List<PMFinding> PencilmarkDataList;

        public RuleFinding()
        {
            PencilmarkDataList = new List<PMFinding>();
        }
    }
}
