namespace SudokuSolver.RuleData
{
    using System.Collections.Generic;
    using GameParts;

    public class SolveInfo
    {
        public string Description { get; set; }
        public List<RuleFinding> Actions { get; set; }

        public SolveInfo()
        {
            Actions = new List<RuleFinding>();
        }

        public void AddAction(Cell cell, CellRole cellRole, params PMFinding[] pmFindings)
        {
            var finding = new RuleFinding() { Cell = cell, CellRole = cellRole };
            finding.PencilmarkDataList.AddRange(pmFindings);
            Actions.Add(finding);
        }
    }
}
