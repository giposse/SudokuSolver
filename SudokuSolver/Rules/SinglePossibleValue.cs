namespace SudokuSolver.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Exceptions;
    using Extensions;
    using GameParts;
    using RuleData;
    using Interfaces;

    public class SinglePossibleValue : SolutionStepBase, ISolutionStep
    {
        public override bool FindPattern(GameBoard board)
        {
            Board = board;
            Cell solutionCell = Board
                .GetAllLines(Coord.ROWS)
                .SelectMany(t => t)
                .FirstOrDefault(cell => cell.PencilMarkCount == 1);

            if (solutionCell == null)
                return false;

            int pmMask = solutionCell.PMSignature;
            int pm = pmMask.ListOfBits()[0];
            Solution = new SolveInfo();
            Solution.AddAction(solutionCell, CellRole.Pattern,
                new PMFinding(pm, PMRole.Solution));

            solutionCell
                .ShadowedCells
                .Where(c => c.IsPencilmarkSet(pm))
                .ToList()
                .ForEach(cell => Solution.AddAction(cell, CellRole.Affected,
                    GetPmFindings(cell, pmMask, PMRole.Remove)));


            string csvPattern = CellsInCsvFormat(solutionCell);
			Solution.Description = $"Only possible value for cell {csvPattern} is {pm}.";

            return true;
        }

        public override void Apply()
        {
            base.Apply();
            foreach (RuleFinding action in Solution.Actions)
                foreach (PMFinding pmFinding in action.PencilmarkDataList)
                    if (pmFinding.Role == PMRole.Solution)
                    {
                        action.Cell.SetValue(pmFinding.Value);
                        return;
                    }

            throw new UnexpectedStateException("Could not find solution cell in rule findings.");
        }

        public override void Undo()
        {
            base.Undo();

            PMFinding pmFinding = null;
            RuleFinding action = Solution.Actions
                .First(t => (pmFinding = t.PencilmarkDataList
                    .FirstOrDefault(finding => finding.Role == PMRole.Solution)) != null);
                    
            action.Cell.ClearCellValue();
            action.Cell.SetPencilMark(pmFinding.Value, true/*pencilMarkActive*/);
        }
    }
}
