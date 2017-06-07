namespace SudokuSolver.Rules
{
	using System.Collections.Generic;
	using System.Linq;
	using Extensions;
	using GameParts;
	using RuleData;

	public class UniqueRectangleRule : SolutionStepBase
    {
        private List<List<Cell>> allRows;
        private List<List<Cell>> allColumns;
        private List<Cell> affectedCells;
        private List<Cell> patternCells;

        public override bool FindPattern(GameBoard board)
        {
            Board = board;
            allRows = board.GetAllLines(Coord.ROWS);
            allColumns = board.GetAllLines(Coord.COLUMNS);
            var first2Candidates = new List<List<Cell>>();
            for (int coord = Coord.ROWS; coord <= Coord.COLUMNS; coord++)
            {
                List<List<Cell>> allLines = Board.GetAllLines(coord);
                first2Candidates.Clear();
                allLines.ForEach(line => line
                    .Where(cell => cell.PMSignature.NumberOfBitsSet() == 2)
                        .GroupBy(cell => cell.PMSignature)
                        .Where(g => g.Count() == 2)
                        .ToList()
                        .ForEach(g => first2Candidates.Add(g.ToList())));

                if (first2Candidates.FirstOrDefault(cellList => MakesUniqueRectangle(cellList)) != null)
                    return true;
            }

            return false;
        }

        private bool MakesUniqueRectangle(List<Cell> firstTwo)
        {
            int keySignature = firstTwo[0].PMSignature;
            string csvAffected, csvPattern, csvHalf2;
            int commonCoord;
            int lineIndex;

            if ((lineIndex = firstTwo[0].RowIndex) == firstTwo[1].RowIndex)
                commonCoord = Coord.ROW;
            else if ((lineIndex = firstTwo[0].ColIndex) == firstTwo[1].ColIndex)
                commonCoord = Coord.COL;
            else
                return false;

            int diffCoord = 1 - commonCoord;
            List<int> cellIndexes = firstTwo
                .Select(cell => cell.GetCoordinate(diffCoord))
                .ToList();

            List<List<Cell>> workLine = (commonCoord == Coord.ROW) ? allRows : allColumns;
            List<List<Cell>> secondHalfLineCandidates = workLine
                .Where(line => line[0].GetCoordinate(commonCoord) != lineIndex &&
                     (line[cellIndexes[0]].PMSignature & keySignature) == keySignature &&
                     (line[cellIndexes[1]].PMSignature & keySignature) == keySignature)
                .ToList();

            foreach(List<Cell> half2Line in secondHalfLineCandidates)
            {
                int half2PencilmarkCount = 0;
                List<Cell> half2 = new List<Cell> { half2Line[cellIndexes[0]], half2Line[cellIndexes[1]] };
                if (firstTwo.Union(half2)
                    .Select(cell => cell.GetCoordinate(Coord.SECTION))
                    .Distinct()
                    .Count() != 2)
                {
                    continue;
                }

                List<Cell> half2ManyPMs = half2.Where(cell => cell.PencilMarkCount > 2).ToList();
                half2.ForEach(cell => half2PencilmarkCount += cell.PencilMarkCount);
                int pmOtherMask;

                // Unique Rectangle - Type 1
                // 3 cells in the rectangle have 2 options, and the 4th cell has more.
                // In the 4th cell, the 2 common options should be turned off.
                if (half2ManyPMs.Count == 1)
                {
                    Solution = new SolveInfo();
                    Cell solutionCell = half2ManyPMs[0];

                    patternCells = new List<Cell>(firstTwo);
                    patternCells.AddRange(half2.Except(half2ManyPMs).ToList());
                    patternCells.ForEach(cell => Solution.AddAction(cell, CellRole.Pattern,
                        GetPmFindings(cell, keySignature, PMRole.Pattern)));

                    affectedCells = half2ManyPMs;
                    Solution.AddAction(solutionCell, CellRole.Pattern, 
                        GetPmFindings(solutionCell, keySignature, PMRole.Remove));

                    patternCells.Add(solutionCell);  // to include in description
                    csvPattern = CellsInCsvFormat(patternCells);
                    csvAffected = CellsInCsvFormat(affectedCells);
                    string csvValues = PMsInCsvFormat(keySignature.ListOfBits());
                    string csvSolutionCell = CellsInCsvFormat(solutionCell);
                    Solution.Description = $"Unique Rectangle - Type 1 : Cell {csvAffected} cannot " +
                        $"have values {csvValues}. Otherwise a deadly pattern would form in cells {csvPattern}. " +
                        $"Therefore these values can be ruled out for that cell.";

                    return true;
                }

                // Possible case of Unique Rectangle - Type 2
                // 2 cells have 3 pencilmarks set (the same 3 for both), so one of the 
                // 2 cells must be the 3rd value.   No other cells in the line (and section)
                // can hold that value.
                if (half2PencilmarkCount == 6 && 
                    half2[0].PMSignature == half2[1].PMSignature)
                {
                    Solution = new SolveInfo();
                    int mask = half2[0].PMSignature ^ keySignature;
                    affectedCells = half2Line.Except(half2)
                        .Where(cell => (cell.PMSignature & mask) != 0)
                        .ToList();

                    if (affectedCells.Count != 0)
                    {
                        List<Cell> affectedInGroup = Board
                            .GetLine(Coord.SECTION, half2[0].GetCoordinate(Coord.SECTION))
                            .Except(half2)
                            .Where(cell => (cell.PMSignature & mask) != 0)
                            .ToList();

                        affectedCells.AddRange(affectedInGroup);
                        patternCells = new List<Cell>(firstTwo);
                        patternCells.AddRange(half2);
                        affectedCells
                            .ForEach(cell => Solution.AddAction(cell, CellRole.Affected,
                                GetPmFindings(cell, mask, PMRole.Remove)));

                        Board.GetLine(commonCoord, lineIndex)
                            .Except(affectedCells)
                            .ToList()
                            .ForEach(cell => Solution.AddAction(cell, CellRole.InvolvedLine));

                        int iValue = mask.ListOfBits()[0];
                        csvPattern = CellsInCsvFormat(patternCells);
                        csvAffected = CellsInCsvFormat(affectedCells);
                        csvHalf2 = CellsInCsvFormat(half2);

						Solution.Description = $"Unique Rectangle - Type 2: Value {iValue} has to " +
							$"be present in one of cells {csvHalf2} to avoid a deadly pattern (where the  puzzle " +
							$"would have 2 solutions). Therefore, value 2 cannot be present in any of " +
							$"cells {csvAffected}.";

                        return true;
                    }
                }

                // Possible case of Unique Rectangle - Type 3
                // 2 cells have 3 or 4 pencilmarks.  The extra pencilmarks are treated as if
                // they were all in a single cell, and a naked pattern is searched for in the line
                // using these extra values.
                // Example: 4 cells have pencilmarks 3,9 - 3,9 - 1,5,3,9 - 1,5,3,9 .  The extra 
                // values (1 and 5) are treated as being in a single cell, and a naked pattern is
                // searched for.  If 2 other cells have (1,4) and (4,5), then the 2 cells and the
                // virtual cell form a naked cell group of size 3 with values 1, 4, 5, and the naked
                // rule is applied accordingly on the line.
                if (half2PencilmarkCount >= 6 && half2PencilmarkCount <= 8)
                {
                    do
                    {
                        int pmsVirtualCell = 0;
                        half2.ForEach(cell => pmsVirtualCell |= cell.PMSignature);
                        pmsVirtualCell ^= firstTwo[0].PMSignature;
                        if (pmsVirtualCell.NumberOfBitsSet() != 2)
                            continue;

                        // clear one of the virtual cells completely
                        List<Cell> virtualLine = half2Line.Select(t => t.Clone()).ToList();

                        // change the pencilmarks in the other cell to match the virtual definition
                        virtualLine[cellIndexes[0]].SetValue(0);  // clear pencilmarks
                        virtualLine[cellIndexes[1]].SetValue(0);
                        virtualLine[cellIndexes[1]].SetMultiplePencilMarks(pmsVirtualCell);

                        // Now perform a search for a naked rule set
                        NakedHiddenFinder finder = new NakedHiddenFinder();
                        finder.MapBits = NakedValuesRule.CreateMapBitsFromLine(virtualLine);
                        finder.MapLength = virtualLine.Count;
                        if (!finder.Search(2/*goal*/) && !finder.Search(3/*goal*/))
                            continue;

                        int pmsVirtualGroup = finder.ActionIndexes
                                .Select(i => half2Line[i].PMSignature)
                                .ToList()
                                .BitMerge();

                        affectedCells = new List<Cell>();
                        Solution = new SolveInfo();

                        pmOtherMask = finder.ActionBits.BitMerge();
                        affectedCells = half2Line
                            .Except(finder.ActionIndexes.Select(i => half2Line[i]).ToList())
                            .Except(half2)
                            .Where(cell => (cell.PMSignature & pmOtherMask) != 0)
                            .ToList();

                        affectedCells.ForEach(cell => Solution.AddAction(cell, CellRole.Affected,
                            GetPmFindings(cell, pmOtherMask, PMRole.Remove)));

                        int deadlySignature = firstTwo[0].PMSignature;
                        firstTwo.ForEach(cell => Solution.AddAction(cell, CellRole.Pattern,
                            GetPmFindings(cell, deadlySignature, PMRole.Pattern)));

                        half2.ForEach(cell => Solution.AddAction(cell, CellRole.Pattern,
                            GetPmFindings(cell,
                            new int[] { deadlySignature, pmsVirtualCell },
                            new PMRole[] { PMRole.Pattern, PMRole.ChainColor1 })));

                        List<Cell> virtualGroup = finder.ActionIndexes
                            .Except(cellIndexes)
                            .Select(i => half2Line[i])
                            .ToList();

                        virtualGroup.ForEach(cell => Solution.AddAction(cell, CellRole.InvolvedLine,
                                GetPmFindings(cell, pmsVirtualGroup, PMRole.ChainColor1)));
                            
                        List<Cell> deadlyCells = firstTwo.Union(half2).ToList();
                        patternCells = deadlyCells
                            .Union(finder.ActionIndexes.Select(indexCell => half2Line[indexCell]))
                            .ToList();

                        List<Cell> line = Board.GetLine(commonCoord, half2[0].GetCoordinate(commonCoord));
                        line
                            .Except(affectedCells)
                            .Except(half2)
                            .Except(virtualGroup)
                            .ToList()
                            .ForEach(cell => Solution.AddAction(cell, CellRole.InvolvedLine));

                        csvAffected = CellsInCsvFormat(affectedCells);
                        csvPattern = CellsInCsvFormat(patternCells);
                        csvHalf2 = CellsInCsvFormat(half2);
                        string csvDeadly = CellsInCsvFormat(deadlyCells);
                        string csvValues = PMsInCsvFormat(pmOtherMask.ListOfBits());
                        string csvPmVirtual = PMsInCsvFormat(pmsVirtualCell.ListOfBits());
                        string commonCoordName = (commonCoord == Coord.ROW) ? "row" : "column";
                        string csvNakedGroup = PMsInCsvFormat((pmOtherMask | pmsVirtualCell).ListOfBits());

                        Solution.Description = string.Format("Unique Rectangle Type 3: Cells {0} form a single " +
                            "virtual cell with pencilmarks {1}. A deadly pattern in cells {2} cannot take " +
                            "place.  One of values {1} has to be present in cells {0} (Naked rule applied " +
                            "using virtual cell for pencilmarks {3}) .  Options {5} can be removed from cells " +
                            "{6}.", csvHalf2, csvPmVirtual, csvDeadly, csvNakedGroup, commonCoordName,
                            csvValues, csvAffected);

                        return true;
                    }
                    while (false);  // builds a common exit point
                }

                // Last case: Unique Rectangle - Type 4
                // 2 cells have many more pencilmarks set. One of the extra values HAS to be
                // the solution for one of the cells.  Then the 4th cell cannot hold the other
                // value (of the deadly rectangle) or else a deadly pattern would form
                if (half2PencilmarkCount < 6)
                    continue;

                List<Cell> analysisCells = half2Line
                    .Except(half2)
                    .ToList();

                List<int> pmAnalysis = keySignature.ListOfBits();
                int pmWork = pmAnalysis.FirstOrDefault(pm => analysisCells.FirstOrDefault(
                    cell => cell.IsPencilmarkSet(pm)) == null);

                if (pmWork == 0)
                    continue;

                Solution = new SolveInfo();
                int pmOther = pmAnalysis.FirstOrDefault(t => t != pmWork);
                affectedCells = half2.Where(cell => cell.IsPencilmarkSet(pmOther)).ToList();
                patternCells = firstTwo.Union(half2).Except(affectedCells).ToList();
                pmOtherMask = 1 << pmOther;
                int pmWorkMask = 1 << pmWork;

                affectedCells.ForEach(cell => Solution.AddAction(cell, CellRole.Pattern,
                    GetPmFindings(cell, 
                        new int[] { pmOtherMask, pmWorkMask }, new PMRole[] { PMRole.Remove, PMRole.Pattern })));

                patternCells.ForEach(cell => Solution.AddAction(cell, CellRole.Pattern,
                    GetPmFindings(cell, pmWorkMask | pmOtherMask, PMRole.Pattern)));

                lineIndex = half2[0].GetCoordinate(commonCoord);
                Board.GetLine(commonCoord, lineIndex)
                    .Except(affectedCells)
                    .ToList()
                    .ForEach(cell => Solution.AddAction(cell, CellRole.InvolvedLine));

                patternCells.AddRange(affectedCells);  // to build description
                csvAffected = CellsInCsvFormat(affectedCells);
                csvPattern = CellsInCsvFormat(patternCells);
                csvHalf2 = CellsInCsvFormat(half2);
                string lineName = (commonCoord == Coord.ROW) ? "row" : "column"; 
                Solution.Description = string.Format("Unique Rectangle - Type 4: Either one of cells {0}" +
                    "must have value {1}.  If the other cell of the two had value {2}, the 4 cells {3} " +
                    "would form a deadly pattern.  Since this cannot happen, cell {4} cannot hold value {2}.",
                    csvHalf2, pmWork, pmOther, csvPattern, csvAffected, lineName);

                return true;
            }

            return false;
        }
    }
}
