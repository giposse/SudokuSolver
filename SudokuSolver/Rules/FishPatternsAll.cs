namespace SudokuSolver.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Exceptions;
    using GameParts;
    using RuleData;

    public class FishPatternsAll : SolutionStepBase
    {
        public List<Cell> FinCells { get; private set; }

        private static string[] FishNames = new string[] { "X-Wing", "Swordfish", "Jellyfish" };

        private int FishSize { get; set; }
        private int MaxCellsPerLine { get; set; }
        private int MaxCellsCumulative { get; set; }
        private bool Finned { get; set; }
        private bool Sashimi { get; set; }

        private List<Cell> AllCells;
        private List<Cell> FinSection { get; set; }

        private List<Cell> patternCells;
        private List<Cell> affectedCells;
        private int affectedPencilmark;

        public FishPatternsAll(int fishSize, bool withFin = false, bool sashimi = false)
        {
            FishSize = fishSize;
            Finned = withFin;
            Sashimi = sashimi;
            if (Sashimi && !Finned)
                throw new ArgumentException("Invalid arguments: Sashimi fish have to be finned by definition.");

            MaxCellsPerLine = fishSize + (Finned ? 2 : 0) - (Sashimi ? 1 : 0);
            MaxCellsCumulative = fishSize + (Finned ? 2 : 0);
        }

        public override bool FindPattern(GameBoard board)
        {
            Board = board;
            AllCells = Board.GetAllLines(Coord.ROWS).SelectMany(t => t).ToList();
            for (int coord = Coord.ROWS; coord <= Coord.COLUMNS; coord++)
            {
                int secCoord = 1 - coord;
                for (int iValue = 1; iValue <= Board.Dimension; iValue++)
                {
                    int count;

                    // IGrouping.  Key is main coordinate, list is list of 2nd coordinates
                    List<IGrouping<int, int>> lineInfo = AllCells
                        .Where(cell => cell.IsPencilmarkSet(iValue))
                        .GroupBy(cell => cell.GetCoordinate(coord), cell => cell.GetCoordinate(secCoord))
                        .Where(g => (count = g.Count()) >= 2 && count <= MaxCellsPerLine)
                        .ToList();

                    if (lineInfo.Count < FishSize)
                        continue;

                    var selectedLines = new List<int>();
                    var selectedSecLines = new List<int>();
                    var selectedFinCells = new List<int[]>();
                    var fishCells = new List<Cell>();
                    if (SelectFish(lineInfo, secCoord, 0/*start*/, iValue,
                        selectedLines, selectedSecLines, selectedFinCells, fishCells))
                    {
                        int pmMask = 1 << affectedPencilmark;

                        Solution = new SolveInfo();
                        affectedCells.ForEach(cell => Solution.AddAction(cell, CellRole.Affected,
                            GetPmFindings(cell, pmMask, PMRole.Remove)));

                        patternCells.ForEach(cell => Solution.AddAction(cell, CellRole.Pattern,
                            GetPmFindings(cell, pmMask, PMRole.Pattern)));

                        if (FinCells == null)
                            FinCells = new List<Cell>();

                        FinCells.ForEach(cell => Solution.AddAction(cell, CellRole.Pattern2,
                            GetPmFindings(cell, pmMask, PMRole.Pattern)));

                        List<int> involvedLineIndexes = patternCells
                            .Select(cell => cell.GetCoordinate(coord))
                            .Distinct()
                            .ToList();


                        involvedLineIndexes
                            .Select(index => Board.GetLine(coord, index))
                            .SelectMany(t => t)
                            .Except(patternCells)
                            .Except(FinCells)
                            .ToList()
                            .ForEach(cell => Solution.AddAction(cell, CellRole.InvolvedLine));

                        string csvCells = CellsInCsvFormat(patternCells);
                        Solution.Description = string.Format("{0}{1} on {2} for pencilmark {4}  Cells involved: {3}.",
                            Finned ? "Finned " : (Sashimi ? "Sashimi " : string.Empty),
                            FishNames[FishSize - 2],
                            coord == Coord.ROWS ? "rows" : "columns",
                            csvCells,
                            iValue);

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Looks for the fish pattern recursively
        /// </summary>
        /// <param name="lineInfo">A List of IGrouping<int, int> where the key is the 
        /// index of the main coordinate (e.g. row) and the list holds the indexes of the
        /// secondary coordinates (e.g. columns). </param>
        /// <param name="secCoord">Indicates if the secondary coordinate is row or column.</param>
        /// <param name="start">Initial index where to start the search in the lineInfo list.</param>
        /// <param name="pmValue">Value of the PencilMark being processed.</param>
        /// <param name="indexesFishLines">List of the indexes of the lines containing pencilmarks which
        /// can be fish.  Initially the list is empty and it fills up as recursion kicks in and the 
        /// different possible combinations are tried.</param>
        /// <param name="indexesSecFishLines">Indexes of the secondary lines of cells considered in the
        /// pattern.  Starts empty and fills as recursion deepens.</param>
        /// <param name="indexesSecFishLinesFreqOne">The indexes of secondary lines which appear only
        /// once in the patterns.  These will help identify fins.</param>
        /// <returns></returns>
        private bool SelectFish(List<IGrouping<int, int>> lineInfo, 
            int secCoord, 
            int start,
            int pmValue,
            List<int> indexesFishLines, 
            List<int> indexesSecFishLines,
            List<int[]> coordsFreqOne,
            List<Cell> fishCells)
        {
            int indexLine;
            int mainCoord = 1 - secCoord;
            List<int> secIndexesConsidered, candidatesMultiFreq = null;

            for (int i = start; i < lineInfo.Count; i++, indexesFishLines.Remove(indexLine))
            {
                // if there are not enough lines to form a fish pattern there's no
                // point in continue searching
                if (indexesFishLines.Count + (lineInfo.Count - i) < FishSize)
                    return false;

                indexLine = lineInfo[i].Key;
                indexesFishLines.Add(indexLine);
                secIndexesConsidered = lineInfo[i].ToList();

                List<int> colsFreqOne = coordsFreqOne.Select(t => t[1]).ToList();
                candidatesMultiFreq = secIndexesConsidered
                    .Intersect(colsFreqOne)
                    .Union(indexesSecFishLines)
                    .ToList();

                int mfCount = candidatesMultiFreq.Count;
                if (mfCount > FishSize)
                    continue;

                // the frequency one candidates plus the new candidates which are not part
                // of multi frequency list are the total candidates for frequency one.
                List<int[]> candidatesFreqOne = coordsFreqOne
                    .Union(secIndexesConsidered.Select(t => new int[] { indexLine, t }))
                    .Where(t => !candidatesMultiFreq.Contains(t[1]))
                    .ToList();

                // those in the same secondary coordinate (e.g. column) count as 1 since they
                // are in the one line of a possible fish.
                int freqOneCount = candidatesFreqOne
                    .Select(t => t[1])
                    .Distinct()  // this should not be necessary
                    .Count();

                if (mfCount + freqOneCount > MaxCellsCumulative)
                    continue;

                if (indexesFishLines.Count < FishSize &&
                    !SelectFish(lineInfo, secCoord, i + 1, pmValue, indexesFishLines, 
                        candidatesMultiFreq, candidatesFreqOne, fishCells))
                {
                    continue;
                }

                // if unwinding recursion and the result is known, no need to recalculate it
                // on every step out.
                if (affectedCells != null && affectedCells.Count > 0)
                    return true;

                /// Start possible templatized solutions.  Simple fish case
                if (!Finned && !Sashimi)
                {
                    if (candidatesFreqOne.Count > 0 || mfCount != FishSize)
                    {
                        indexesFishLines.Remove(indexLine);
                        return false;
                    }

                    affectedCells = AllCells
                        .Where(cell => candidatesMultiFreq.Contains(cell.GetCoordinate(secCoord)) &&
                            !indexesFishLines.Contains(cell.GetCoordinate(mainCoord)))
                        .Where(cell => cell.IsPencilmarkSet(pmValue))
                        .ToList();

                    if (affectedCells.Count == 0)
                        continue;

                    patternCells = AllCells
                        .Where(cell => indexesFishLines.Contains(cell.GetCoordinate(mainCoord)) &&
                            candidatesMultiFreq.Contains(cell.GetCoordinate(secCoord)))
                        .ToList();

                    affectedPencilmark = pmValue;
                    return true;
                }

                // The following is common for Finned or Sashimi
                if (freqOneCount < 1 || freqOneCount > (Sashimi ? 3 : 2))
                    continue;

                int rowIndex = (mainCoord == Coord.ROWS) ? 0 : 1;
                int colIndex = 1 - rowIndex;
                FinCells = candidatesFreqOne
                    .Select(t => Board.GetCell(t[rowIndex], t[colIndex]))
                    .ToList();

                if (Finned && !Sashimi && 
                    (
                        (freqOneCount == 2 && 
                            (FinCells[0].GetCoordinate(mainCoord) != FinCells[1].GetCoordinate(mainCoord) ||
                            FinCells[0].GetCoordinate(Coord.SECTION) != FinCells[1].GetCoordinate(Coord.SECTION)))
                    ||
                        !VerifyFishFin(candidatesMultiFreq, secCoord, pmValue))
                    )
                {
                    continue;
                }

                if (Sashimi)
                {
                    // if there is only one frequency-one cell, it has to be the fin.
                    if (freqOneCount == 1 && 
                        !VerifyFishFin(candidatesMultiFreq, secCoord, pmValue))
                    {
                        continue;
                    }

                    // if there are 2, there are several options
                    //  a) None are fins.
                    //  b) Both are fins
                    //  c) F1 is a fin and F2 is not 
                    //  d) F2 is a fin and F1 is not
                    // More than one statement can be true in some cases.  If so, select the 
                    // first one found where pencilmarks can be turned OFF.
                    if (freqOneCount == 2)
                    {
                        var finCandidateInfo = candidatesFreqOne
                            .Select(cfo => new 
                            { 
                                Cell = Board.GetCell(cfo[rowIndex], cfo[colIndex]),
                                IsScale = candidatesMultiFreq.Contains(cfo[1])
                            }).ToList();

                        int sureScaleCount = finCandidateInfo.Where(fc => fc.IsScale).Count();

                        // a) None are fins
                        if (sureScaleCount == 2 || mfCount + 2/*freqOneCount*/ == FishSize)
                            continue;

                        // b) Both are fins
                        if (mfCount == FishSize)
                        {
                            FinCells = finCandidateInfo
                                .Where(fc => !fc.IsScale)
                                .Select(fc => fc.Cell)
                                .ToList();

                            if (FinCells.Count != 2 ||
                                !VerifyFishFin(indexesSecFishLines, secCoord, pmValue))
                            {
                                continue;
                            }
                        }

                        // c) One is a fin, and one is a scale
                        // c-i) The scale is known to be a scale
                        if (sureScaleCount == 1)
                        {
                            FinCells = finCandidateInfo
                                .Where(fc => !fc.IsScale)
                                .Select(fc => fc.Cell)
                                .ToList();

                            if (FinCells.Count != 1)
                                throw new UnexpectedStateException("Number of fin cells should be 1.");

                            if (!VerifyFishFin(indexesSecFishLines, secCoord, pmValue))
                                continue;
                        }
                        else if (sureScaleCount == 0)   // c-ii) Fin and Scale may be interchangeable
                        {
                            if (mfCount != FishSize - 1)
                                continue;

                            bool foundFin = false;
                            for (int finChance = 0; finChance < 2; finChance++)
                            {
                                FinCells.Clear();
                                FinCells.Add(finCandidateInfo[finChance].Cell);
                                Cell scaleCandidate = finCandidateInfo[1 - finChance].Cell;
                                var tempSecFishLines = indexesSecFishLines
                                    .Union(new List<int> { scaleCandidate.GetCoordinate(secCoord) })
                                    .ToList();

                                if ((foundFin = VerifyFishFin(indexesSecFishLines, secCoord, pmValue)))
                                    break;
                            }

                            if (!foundFin)
                                continue;
                        }

                        var otherScale = finCandidateInfo
                            .FirstOrDefault(fci => !FinCells.Contains(fci.Cell));

                        candidatesMultiFreq.Add(otherScale.Cell.GetCoordinate(secCoord));
                    }

                    // if there are 3, then 2 are fins on the same line
                    if (freqOneCount == 3)
                    {
                        List<IGrouping<int, int[]>> groupedSingles = candidatesFreqOne
                            .GroupBy(t => t[0])
                            .ToList();


                        IGrouping<int, int[]> finData = groupedSingles
                            .FirstOrDefault(g => g.Count() == 2);

                        IGrouping<int, int[]> scaleData = groupedSingles
                            .FirstOrDefault(g => g.Count() == 1);

                        if (finData == null)
                            continue;

                        if (scaleData == null)
                            throw new UnexpectedStateException("Expected scale or Sashimi cell not found.");

                        FinCells = finData
                            .Select(t => Board.GetCell(t[rowIndex], t[colIndex]))
                            .ToList();

                        if (FinCells.Select(fc => fc.GetCoordinate(Coord.SECTION))
                            .Distinct()
                            .Count() > 1)
                        {
                            continue;
                        }

                        candidatesMultiFreq.Add(scaleData.ToList()[0][1]);

                        if (!VerifyFishFin(candidatesMultiFreq, secCoord, pmValue))
                            continue;
                    }
                }

                affectedCells = FinSection
                   . Where(cell => candidatesMultiFreq.Contains(cell.GetCoordinate(secCoord))
                        && !indexesFishLines.Contains(cell.GetCoordinate(mainCoord))
                        && cell.IsPencilmarkSet(pmValue))
                    .ToList();

                if (affectedCells.Count == 0)
                    continue;

                patternCells = AllCells
                    .Where(cell => indexesFishLines.Contains(cell.GetCoordinate(mainCoord)) &&
                        candidatesMultiFreq.Contains(cell.GetCoordinate(secCoord)))
                    .ToList();

                affectedPencilmark = pmValue;
                return true;

            }

            affectedCells = null;
            return false;
        }

        /// <summary>
        /// Method to verify that the fincells do share a group with the fish, and are not 
        /// unexpectedly just single cells somewhere far away from the fish.
        /// </summary>
        /// <param name="finCell"></param>
        /// <param name="fishSecCoords"></param>
        /// <param name="secCoord"></param>
        /// <returns></returns>
        private bool VerifyFishFin(List<int> fishSecCoords, int secCoord, int pmValue)
        {
            int mainCoord = 1 - secCoord;
            Cell finCell = FinCells[0];
            int finLine = finCell.GetCoordinate(mainCoord);
            FinSection = Board.GetLine(Coord.SECTION, finCell.GetCoordinate(Coord.SECTION));

            List<Cell> sectionFinLine = FinSection
                .Where(cell => cell.GetCoordinate(mainCoord) == finLine)
                .Except(FinCells)
                .ToList();

            // if just fin, a cell contains pencilmark
            bool returnValue = FinSection
                .FirstOrDefault(cell => fishSecCoords.Contains(cell.GetCoordinate(secCoord)) &&
                    Sashimi != cell.IsPencilmarkSet(pmValue)) != null;

            return returnValue;
        }
    }
}
