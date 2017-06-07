namespace SudokuSolver.Rules
{
    using System.Collections.Generic;
    using System.Linq;
    using GameParts;
    using RuleData;

    /// <summary>
    /// This rule wil check for locked cells in a single row.   It works like this:
    /// If a row is PP# xxx  xxx, where all pencilmarks P are in the same section (group),
    /// Then the only options for these pencilmarks is to live in the row, or else the row
    /// would not have those numbers.   Therefore, all pencilmarks P can be turned off in the 
    /// section.  Like this
    /// 
    /// Pxx                             xxx
    /// PP#   ==>   this will become    PP#
    /// #xP                             #xx
    /// 
    /// Basically pencilmark P has been turned off in all the section, except row 2.
    /// 
    /// The method is generic enough to apply the rule using:
    ///    Check locked rows and unmark PMs in Sections (as described above)
    ///    Check locked columns and unmark PMs in Sections (same as above, but with columns)
    ///    Check locked groups, and unmark PMs either in Rows or Cols
    /// </summary>
    public class LockedInLinesOrGroups : SolutionStepBase
    {
        private static string[] LineNames = new string[] { "row", "column", "box" };
        private List<Cell> affectedCells;
        private List<Cell> patternCells;
        int PMPattern;

        public override bool FindPattern(GameBoard board)
        {
            Board = board;
            for (int coord = Coord.ROW; coord <= Coord.SECTION; coord++)
            {
                List<List<Cell>> allLines = board.GetAllLines(coord);
                bool inSections = coord == Coord.SECTION;
                int secCoord = inSections ? Coord.ROW : 1 - coord;
                int thirdCoord = inSections ? Coord.COL : Coord.SECTION;
                for (int iValue = 1; iValue <= board.Dimension; iValue++)
                {
                    Solution = new SolveInfo();
                    List<Cell> singleLine = 
                        allLines.FirstOrDefault(line => HasLockedCells(line, iValue, thirdCoord) ||
                             (coord == Coord.SECTION && HasLockedCells(line, iValue, secCoord)));

                    if (singleLine != null)
                    {
                        int pmMask = 1 << iValue;
                        string csvCells = CellsInCsvFormat(patternCells);
                        string csvAffected = CellsInCsvFormat(affectedCells);
                        int involvedCoordinate = coord;
                        int lineIndex = singleLine[0].GetCoordinate(coord);

                        affectedCells.ForEach(cell => Solution.AddAction(cell, CellRole.Affected,
                            GetPmFindings(cell, pmMask, PMRole.Remove)));

                        patternCells.ForEach(cell => Solution.AddAction(cell, RuleData.CellRole.Pattern,
                            GetPmFindings(cell, pmMask, PMRole.Pattern)));

                        Board.GetLine(involvedCoordinate, lineIndex)
                            .Except(patternCells)
                            .ToList()
                            .ForEach(cell => Solution.AddAction(cell, RuleData.CellRole.InvolvedLine));

                        Solution.Description = string.Format("Locked Cells Rule: One of cells {0} must have " +
                            "solution value {3}.  All cells share a {1} " +
                            " and a {2}. Therefore no other cell in that {2} could possibly have that " +
                            "value (or else no cell in the {1} could have the value). " + 
                            "This means value {3} can be ruled out for cells {4}.",
                            csvCells, LineNames[coord], LineNames[thirdCoord], iValue, csvAffected);

                        return true;
                    }
                }
            }

            return false;
        }

        private bool HasLockedCells(List<Cell> line, int iValue, int secCoord)
        {
            int totalCandidates = 0;
            patternCells = line.Where(cell => cell.IsPencilmarkSet(iValue)).ToList();
            if ((totalCandidates = patternCells.Count) == 0)
                return false;

            int secIndex = patternCells[0].GetCoordinate(secCoord);
            if (patternCells
                .Where(cell => cell.GetCoordinate(secCoord) == secIndex)
                .Count() != totalCandidates)
            {
                return false;
            }

            affectedCells = Board
                .GetLine(secCoord, patternCells[0].GetCoordinate(secCoord))
                .Except(patternCells)
                .Where(cell => cell.IsPencilmarkSet(iValue))
                .ToList();

            PMPattern = iValue;
            return (affectedCells.Count > 0);
        }
    }
}
