namespace SudokuSolver.Rules
{
	using System.Collections.Generic;
	using System.Linq;
	using Extensions;
	using GameParts;
	using RuleData;

	public class XYZWingRule : SolutionStepBase
    {
        List<Cell> affectedCells;

        public override bool FindPattern(GameBoard board)
        {
            Board = board;
            List<List<Cell>> allGroups = Board.GetAllLines(Coord.SECTION);
            foreach (List<Cell> group in allGroups)
            {
                List<Cell> pivotCandidates = group
                    .Where(cell => cell.PMSignature.NumberOfBitsSet() == 3)
                    .ToList();

                int groupIndex = group[0].GetCoordinate(Coord.SECTION);
                foreach (Cell singlePivot in pivotCandidates)
                {
                    List<Cell> pincer1Candidates = group.Where(cell =>
                        (cell.PMSignature & singlePivot.PMSignature).NumberOfBitsSet() == 2 &&
                            cell.PMSignature.NumberOfBitsSet() == 2)
                        .ToList();

                    foreach (Cell pincer1Cand in pincer1Candidates)
                    {
                        int mask = pincer1Cand.PMSignature ^ singlePivot.PMSignature;
                        for (int coord = Coord.ROW; coord <= Coord.COL; coord++)
                        {
                            int lineIndex = singlePivot.GetCoordinate(coord);
                            if (lineIndex == pincer1Cand.GetCoordinate(coord))
                                continue;

                            List<Cell> pincer2Candidates = Board.GetLine(coord, lineIndex)
                                .Where(cell => 
                                    cell.GetCoordinate(Coord.SECTION) != groupIndex &&
                                    cell.PMSignature != pincer1Cand.PMSignature &&
                                        cell.PencilMarkCount == 2 &&
                                        (cell.PMSignature & singlePivot.PMSignature).NumberOfBitsSet() == 2)
                                .ToList();

                            foreach (Cell pincer2Cand in pincer2Candidates)
                            {
                                int commonPMSignature = singlePivot.PMSignature ^ pincer2Cand.PMSignature ^ pincer1Cand.PMSignature;
                                int commonPM = commonPMSignature.ListOfBits()[0];

                                affectedCells = group
                                    .Except(new List<Cell> { singlePivot })
                                    .Intersect(pincer2Cand.ShadowedCells)
                                    .Where(cell => cell.IsPencilmarkSet(commonPM))
                                    .ToList();

                                if (affectedCells.Any())
                                {
                                    Solution = new SolveInfo();
                                    Solution.AddAction(singlePivot, CellRole.Pattern, 
                                        GetPmFindings(singlePivot, commonPMSignature, PMRole.Pattern));

                                    new List<Cell> { pincer1Cand, pincer2Cand }
                                        .ForEach(cell => Solution.AddAction(cell, CellRole.Pattern2,
                                            GetPmFindings(cell, commonPMSignature, PMRole.Pattern)));

                                    affectedCells.ForEach(cell => Solution.AddAction(cell, CellRole.Affected,
                                        GetPmFindings(cell, commonPMSignature, PMRole.Remove)));

                                    Board.GetLine(coord, lineIndex)
                                        .Except(new List<Cell> { singlePivot, pincer1Cand, pincer2Cand })
                                        .Except(affectedCells)
                                        .ToList()
                                        .ForEach(cell => Solution.AddAction(cell, CellRole.InvolvedLine));

                                    string csvPivot = CellsInCsvFormat(singlePivot);
                                    string csvPincers = CellsInCsvFormat(pincer1Cand, pincer2Cand);
                                    string csvAffected = CellsInCsvFormat(affectedCells);

                                    Solution.Description = string.Format("XYZ Wing Rule: Pivot cell {0} with pincer cells {1}. " +
                                        "Since 1 of the 3 cells must have common candidate {2}, then cells shadowed by " +
                                        "all 3 cells, and having the common candidate {2} ({3}), can have the candidate removed.", 
                                        csvPivot, csvPincers, commonPM, csvAffected);

                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
