namespace SudokuSolver.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using GameParts;
    using RuleData;

    public class ALSxzRule : SolutionStepBase
    {
        #region ISolutionStep implementation

        public override bool FindPattern(GameBoard board)
        {
            Board = board;
            var allALS = new List<AlmostLockedSet>();
            for (int coord = Coord.ROW; coord <= Coord.SECTION; coord++)
            {
                Board.GetAllLines(coord)
                    .ForEach(line => allALS.AddRange(
                        AlmostLockedSet.CreateALSsFromLine(line, coord)));
            }

            int alsCount = allALS.Count;
            ALSMatch match;
            for (int i = 0; i < alsCount; i++)
                for (int j = i + 1; j < alsCount; j++)
                {
                    if ((match = AlmostLockedSet.AreAlsXZ(allALS[i], allALS[j])) == null)
                        continue;

                    Solution = new SolveInfo();
                    int commonMask = 1 << match.commonPencilmark;
                    int restrictedMask = 1 << match.commonRestrictedPencilmark;

                    match.AffectedCells
                        .ForEach(cell => Solution.AddAction(cell, CellRole.Affected, 
                            GetPmFindings(cell, commonMask, PMRole.Remove)));

                    int pmMask = commonMask | restrictedMask;
                    match.Als1.Cells.ForEach(cell => Solution.AddAction(cell, CellRole.Pattern, 
                        GetPmFindings(cell, 
                        new int[] { commonMask, restrictedMask },
                        new PMRole[] { PMRole.ChainColor1, PMRole.ChainColor2 })));

                    match.Als2.Cells.ForEach(cell => Solution.AddAction(cell, CellRole.Pattern2,
                        GetPmFindings(cell, 
                            new int[] { commonMask, restrictedMask },
                            new PMRole[] { PMRole.ChainColor1, PMRole.ChainColor2 })));

                    string csvALS1 = CellsInCsvFormat(match.Als1.Cells);
                    string csvALS2 = CellsInCsvFormat(match.Als2.Cells);
                    string csvAffected = CellsInCsvFormat(match.AffectedCells);
                    string csvCommonCellsAls1 = CellsInCsvFormat(match.Als1.Cells
                        .Where(cell => cell.IsPencilmarkSet(match.commonPencilmark))
                        .ToList());

                    string csvCommonCellsAls2 = CellsInCsvFormat(
                        match.Als2.Cells
                            .Where(cell => cell.IsPencilmarkSet(match.commonPencilmark))
                            .ToList());

                    int pmCount1 = match.Als1.Cells.Count + 1;
                    int pmCount2 = match.Als2.Cells.Count + 1;

                    Solution.Description = string.Format("ALS - XZ Rule: Cells {0} have a total " +
                        "of {1} pencilmarks.  Cells {2} have a total of {3} pencilmarks. " +
                        "Pencilmark {4} is common and exclusive to both groups since they all " +
                        "can see each other.  Pencilmark {5} will either be in cells {6} or cells {7}." +
                        "Since cell(s) {8} are shadowed by all these, then option {5} can be removed " +
                        "in these cells.", csvALS1, pmCount1, csvALS2, pmCount2,
                        match.commonRestrictedPencilmark, match.commonPencilmark,
                        csvCommonCellsAls1, csvCommonCellsAls2, csvAffected);

                    return true;
                }

            return false;
        }

        #endregion

    }
}
