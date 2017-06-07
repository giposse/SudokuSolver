namespace SudokuSolver.Rules
{
	using System.Linq;
	using System.Collections.Generic;
	using Extensions;
	using GameParts;
	using RuleData;

	public class RemotePair : SolutionStepBase
    {
        // This rule is as follows:  The pattern is a chain of cells with only 2 possible values
        // A and B.  If the number of cells in the chain is even, then the values in the cells will 
        // be A-B-A-B.....A-B or flipped.   The last cell has a value different from the first cell.
        // That means the first and last values in the chain will be A and B.  Therefore any cells
        // shadowed by both the start and end of the chain cannot have values A or B.
        public override bool FindPattern(GameBoard board)
        {
            Board = board;
            IGrouping<int, Cell> remotePairPath = board
                .GetAllLines(Coord.ROWS)
                .SelectMany(t => t)
                .Where(t => t.PMSignature.NumberOfBitsSet() == 2)
                .GroupBy(t => t.PMSignature)
                .Where(g => g.Count() >= 3)
                .FirstOrDefault(g => IsPathWithRemotePair(g.ToList()));

            return remotePairPath != null;
        }

        public bool IsPathWithRemotePair(List<Cell> path)
        {
            var orderedPath = path.Take(1).ToList();
            path.RemoveAt(0);
            Cell lastCell = orderedPath[0];
            Cell firstCell = lastCell;
            List<Cell> affectedCells = null;
            bool solved = false;
            int pmMask = firstCell.PMSignature;
            while (path.Count > 0)
            {
                Cell nextCell = path.FirstOrDefault(t => lastCell.ShadowedCells.Contains(t));
                if (nextCell == null)
                    return false;

                lastCell = nextCell;
                orderedPath.Add(nextCell);
                path.Remove(nextCell);
                if ((orderedPath.Count & 0x01) == 0x01)
                    continue;

                affectedCells = firstCell
                    .ShadowedCells
                    .Intersect(lastCell.ShadowedCells)
                    .Where(t => (t.PMSignature & pmMask) != 0)
                    .ToList();

                if (affectedCells.Count == 0)
                    continue;

                solved = true;
                break;
            }

            if (solved)
            {
                Solution = new SolveInfo();
                PMRole[] chainRoles = new[] { PMRole.ChainColor1, PMRole.ChainColor2 };
                int linkIndex = 0;
                List<int> optionPair = pmMask.ListOfBits();
                foreach (Cell pathCell in orderedPath)
                {
                    Solution.AddAction(pathCell, CellRole.Pattern,
                        new PMFinding(optionPair[0], chainRoles[linkIndex]),
                        new PMFinding(optionPair[1], chainRoles[linkIndex]));

                    linkIndex = 1 - linkIndex;
                }

                affectedCells.ForEach(cell => Solution.AddAction(cell, CellRole.Affected,
                    GetPmFindings(cell, pmMask, PMRole.Remove)));

                string csvCells = CellsInCsvFormat(orderedPath, false);
                string csvAffected = CellsInCsvFormat(affectedCells);
                Solution.Description = $"Remote Pair Rule: cells {csvCells} can only have values " +
					$"{optionPair[0]} or {optionPair[1]} and form a chain with " +
                    $"an even number of cells.  Therefore the first and last cells in the chain will " +
                    $"contain both values, and cells csvAffected (seen by these 2) could not possible have any " +
                    $"of the 2 values.";
            }

            return solved;
        }
    }
}
