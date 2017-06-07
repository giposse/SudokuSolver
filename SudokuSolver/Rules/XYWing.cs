namespace SudokuSolver.Rules
{
	using System.Collections.Generic;
	using System.Linq;
	using Extensions;
	using GameParts;
	using RuleData;

	public class XYWingPattern : SolutionStepBase
    {
        private List<Cell> patternCells;
        private List<Cell> affectedCells;

        public override bool FindPattern(GameBoard board)
        {
            Board = board;
            IEnumerable<Cell> cellsWith2PMs = board
                .GetAllLines(Coord.ROWS)
                .SelectMany(cell => cell)
                .Where(cell => cell.PencilMarkCount == 2)
                .ToList();

            var pincer = cellsWith2PMs
                .Select((v, i) => new { Value = v, Index = i })
                .FirstOrDefault(c1 => 
                    cellsWith2PMs
                        .Select((v, i) => new { Value = v, Index = i })
                        .FirstOrDefault(c2 => c1.Index < c2.Index && FindXYWing(c1.Value, c2.Value)) != null);

            if (pincer == null)
                return false;

            return true;
        }

        private bool FindXYWing(Cell cell1, Cell cell2)
        {
            Cell Pincer1 = cell1;
            Cell Pincer2 = cell2;
            int commonPMSignature = (cell1.PMSignature & cell2.PMSignature);
            List<int> commonPMs = commonPMSignature.ListOfBits();
            int pivotSignature = cell1.PMSignature ^ cell2.PMSignature;
            if (cell1 == cell2 || commonPMs.Count != 1 || pivotSignature.NumberOfBitsSet() != 2)
                return false;

            IEnumerable<Cell> shadowedZone = cell1.ShadowedCells
                .Intersect(cell2.ShadowedCells);

            affectedCells = shadowedZone.Where(c => (c.PMSignature & commonPMSignature) != 0).ToList();
            if (affectedCells.Count == 0)
                return false;

            Cell Pivot = null;
            bool returnValue = 
                (cell1.PMSignature | cell2.PMSignature).NumberOfBitsSet() == 3 &&
                cell1.ColIndex != cell2.ColIndex && cell1.RowIndex != cell2.RowIndex &&
                cell1.GroupIndex != cell2.GroupIndex && 
                (Pivot = shadowedZone.FirstOrDefault(c => c.PMSignature == pivotSignature)) != null;

            patternCells = new List<Cell> { Pincer1, Pivot, Pincer2 };
            if (returnValue)
            {
                Solution = new SolveInfo();
                new List<Cell> { Pincer1, Pincer2 }
                    .ForEach(cell => Solution.AddAction(cell, CellRole.Pattern2,
                        GetPmFindings(cell, commonPMSignature, PMRole.Pattern)));

                Solution.AddAction(Pivot, CellRole.Pattern, GetPmFindings(Pivot, pivotSignature, PMRole.Pattern));

                affectedCells.ForEach(cell => Solution.AddAction(cell, CellRole.Affected,
                    GetPmFindings(cell, commonPMSignature, PMRole.Remove)));

                string csvPivot = CellsInCsvFormat(Pivot);
                string csvAffected = CellsInCsvFormat(affectedCells);
                string csvPincers = CellsInCsvFormat(Pincer1, Pincer2);
                List<int> pivotPMs = pivotSignature.ListOfBits().ToList();
                Solution.Description = $"XY-Wing Rule:  No matter what the value of cell {csvPivot} is " +
                    $"(either {pivotPMs[0]} or {pivotPMs[1]}), cell {csvAffected} cannot possibly have value {csvPincers} " +
					$"because one of cells {csvPincers} will have that value.";
            }

            return returnValue;
        }
    }
}
