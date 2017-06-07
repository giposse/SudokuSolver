namespace SudokuSolver.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Exceptions;
    using Extensions;
    using GameParts;
    using RuleData;

    public class SimpleAndMedusaColoring : SolutionStepBase
    {
        protected Dictionary<Cell, List<Cell>> CellGraph;
        private List<Cell> VisitedCells;  // cell from graph already visited.
        protected List<Cell> AllBoardCells;
        protected PMColor? ContradictionColor;

        private const int INDEX_ROWS = 0;
        private const int INDEX_COLUMNS = 1;
        private const int INDEX_SECTIONS = 2;
        private List<List<List<Cell>>> CellLineGroups;

        private bool MedusaOnly;

        public SimpleAndMedusaColoring(bool medusa)
        {
            MedusaOnly = medusa;
        }

        protected void InitCellLineGroups()
        {
            CellLineGroups = new List<List<List<Cell>>> 
                {
                    Board.GetAllLines(Coord.ROWS),
                    Board.GetAllLines(Coord.COLUMNS),
                    Board.GetAllLines(Coord.SECTIONS)
                };
        }

        public override bool FindPattern(GameBoard board)
        {
            Board = board;
            InitCellLineGroups();
            int dim = Board.Dimension;
            AllBoardCells = Board.GetAllLines(Coord.ROWS)
                .SelectMany(t => t)
                .ToList();

            var colorizedCells = new List<Cell>();
            List<Cell> affectedCells = null;
            for (int pm = 1; pm <= dim; pm++)
            {
                Board.ClearPMColorization();
                Logger.Clear();
                GetConjugateSegments(pm);

                while (CellGraph.Count > 0)
                {

                    #region Cleaning for loop 2 and over

                    colorizedCells = Board.GetAllColorizedCells();
                    Logger.WriteLine("Colorized cells are: {0}.", CellsInCsvFormat(colorizedCells, false));
                    StringBuilder finalDescription = new StringBuilder();

                    colorizedCells
                        .Where(cell => VisitedCells.Contains(cell))
                        .ToList()
                        .ForEach(cell => CellGraph.Remove(cell));

                    Logger.WriteLine("Removed colorized from CellGraph.  Remaining cells are {0}.",
                        CellsInCsvFormat(CellGraph.Keys.ToList()));

                    Board.ClearPMColorization();
                    colorizedCells.Clear();
                    VisitedCells.Clear();

                    #endregion

                    ColorizeSegments(pm);
                    colorizedCells = Board.GetAllColorizedCells();

                    if (colorizedCells.Count < 4)
                        continue;

                    if (!MedusaOnly)
                    {
                        #region "Simple Coloring - Rule 2"

                        // Rule 2: If any 2 cell in the same unit have the same color, all such colored
                        // pencilmarks can be eliminated.
                        foreach (var colorValue in new[] { PMColor.COLOR1, PMColor.COLOR2 })
                        {
                            List<Cell> commonColorCells = colorizedCells
                                .Where(cell => cell.GetPencilmarkColor(pm) == colorValue)
                                .ToList();

                            for (int coord = Coord.ROWS; coord <= Coord.SECTIONS; coord++)
                            {
                                IGrouping<int, Cell> crowdedGroup = commonColorCells
                                    .GroupBy(cell => cell.GetCoordinate(coord))
                                    .FirstOrDefault(group => group.Count() > 1);

                                if (crowdedGroup != null)
                                {
                                    List<Cell> badColorCells = commonColorCells;
                                    List<Cell> goodColorCells = colorizedCells
                                        .Except(badColorCells)
                                        .ToList();

                                    Solution = new SolveInfo();
                                    badColorCells.ForEach(cell => Solution.AddAction(cell, CellRole.Pattern,
                                        GetPmFindings(cell, 1 << pm, PMRole.ChainColor2)));  // PMRole.Remove

                                    goodColorCells.ForEach(cell => Solution.AddAction(cell, CellRole.Pattern2,
                                        GetPmFindings(cell, 1 << pm, PMRole.Pattern)));

                                    string csvAffected = CellsInCsvFormat(badColorCells);
                                    Solution.Description = $"Simple Coloring - Type 1: All sets of cells " +
                                        $"where a pencilmark appears twice in a row, column or box are found.  Then a " +
                                        $"graph is made alternating colors.  Once done, any row, column or box having " +
                                        $"2 or more cells of the same color indicate that the color cannot happen and  " +
                                        $"should be removed.  Option {pm} will be removed for cells {csvAffected}.";

                                    return true;
                                }
                            }
                        }

                        #endregion

                        #region "Simple Coloring - Rule 4"

                        // Rule 4:  If any cell in the board, not included in the graph can see
                        // 2 pencilmarks of the same color, then that cell can be removed.
                        do
                        {
                            List<IGrouping<PMColor, Cell>> groupsByColor =
                                colorizedCells.GroupBy(cell => cell.GetPencilmarkColor(pm))
                                .ToList();

                            if (groupsByColor.Count != 2)
                                continue;

                            List<Cell> color1Cells = groupsByColor[0].ToList();
                            List<Cell> color2Cells = groupsByColor[1].ToList();

                            affectedCells = AllBoardCells
                                .Except(colorizedCells)
                                .Where(cell => cell.IsPencilmarkSet(pm) &&
                                cell.GetShadowedCellsInList(color1Cells, pm).Any() &&
                                cell.GetShadowedCellsInList(color2Cells, pm).Any())
                                .ToList();

                            if (affectedCells.Any())
                            {
                                int pmMask = 1 << pm;
                                Solution = new SolveInfo();
                                color1Cells.ForEach(cell => Solution.AddAction(cell, CellRole.Pattern,
                                    GetPmFindings(cell, pmMask, PMRole.ChainColor1)));

                                color2Cells.ForEach(cell => Solution.AddAction(cell, CellRole.Pattern,
                                    GetPmFindings(cell, pmMask, PMRole.ChainColor2)));

                                affectedCells.ForEach(cell => Solution.AddAction(cell, CellRole.Affected,
                                    GetPmFindings(cell, pmMask, PMRole.Remove)));

                                string csvAffected = CellsInCsvFormat(affectedCells);
                                string csvChain = CellsInCsvFormat(colorizedCells.ToList(), false/*sorted*/);
                                Solution.Description = $"Simple Coloring - Type 2: Only 2 cells in a " +
                                    $"housing with a pencilmark, form a segment.  All segments are joined in a graph, " +
                                    $"and then cells are colored alternating color.  Any cell in the board which can " +
                                    $"'see' cells of both colors can be eliminated as a valid option.   Cells {csvAffected} " +
                                    $"can see cells of both colors in chain {csvChain}.  Pencilmark {pm} can be removed from them.";

                                return true;
                            }
                        }
                        while (false);

                        #endregion

                        continue;
                    }

                    #region "Medusa Coloring Rules"

                    bool medusaRuleFound = false;
                    ColorizeMedusa(pm);
                    do
                    {

                        #region Medusa - Rule 0: Contradiction found

                        Cell contradictionCell = colorizedCells.FirstOrDefault(cell => cell.ClashedPMColor != PMColor.NONE);
                        if (contradictionCell != null)
                        {
                            string csvCellContradiction = CellsInCsvFormat(contradictionCell);

                            finalDescription.AppendFormat("Medusa contradicted color rule: Following a path of alternating " +
                                "colors, led to cell {0} where pencilmark {1} would have both colors.  Therefore color " +
                                "{2} is not a solution and can be eliminated.\r\n", csvCellContradiction, contradictionCell.ClashedPM,
                                contradictionCell.ClashedPMColor);

                            FalseColorProcessing(colorizedCells, finalDescription, contradictionCell.ClashedPMColor);
                            medusaRuleFound = true;
                            break;
                        }

                        #endregion

                        #region Medusa - Rule 1: PM color twice in a cell

                        colorizedCells = Board.GetAllColorizedCells();
                        PMColor duplicateColor = PMColor.NONE;
                        Cell dupColorCell = colorizedCells
                            .FirstOrDefault(cell => (duplicateColor = cell.DuplicatePMColor) != PMColor.NONE);

                        if (dupColorCell != null)
                        {
                            List<int> listOfColorPMs = dupColorCell.ListOfColorizedPencilmarks();
                            string csvColorPMs = PMsInCsvFormat(listOfColorPMs);
                            string csvDupCell = CellsInCsvFormat(dupColorCell);
                            finalDescription.AppendFormat("Rule 1: Cell {0} has pencilmarks {1}, both with color {2}, so the color " +
                                "can be eliminated as possible solution in the board.", csvDupCell, csvColorPMs, duplicateColor);

                            FalseColorProcessing(colorizedCells, finalDescription, duplicateColor);
                            medusaRuleFound = true;
                            break;
                        }

                        #endregion

                        #region Medusa - Rule 2: PMs with same value and same color in the same house

                        int len = colorizedCells.Count;
                        for (int iCell = 0; !medusaRuleFound && iCell < len; iCell++)
                        {
                            Cell colorCell = colorizedCells[iCell];
                            List<int> colorPMs = colorCell.ListOfColorizedPencilmarks();
                            for (int iCell2 = iCell + 1; !medusaRuleFound && iCell2 < len; iCell2++)
                            {
                                Cell colorCell2 = colorizedCells[iCell2];
                                if (!colorCell2.CanSee(colorCell))
                                    continue;

                                foreach (int singleColorPM in colorPMs)
                                {
                                    PMColor pm1Color = colorCell.GetPencilmarkColor(singleColorPM);
                                    PMColor pm2Color = colorCell2.GetPencilmarkColor(singleColorPM);
                                    if (!colorCell2.IsPencilmarkColorized(singleColorPM) ||
                                        pm2Color != colorCell.GetPencilmarkColor(singleColorPM))
                                    {
                                        continue;
                                    }

                                    string csvColorCell = CellsInCsvFormat(colorCell);
                                    string csvColorCell2 = CellsInCsvFormat(colorCell2);
                                    string commonCoordName = null;
                                    if (colorCell.RowIndex == colorCell2.RowIndex)
                                        commonCoordName = "row";
                                    else if (colorCell.ColIndex == colorCell2.ColIndex)
                                        commonCoordName = "column";
                                    else
                                        commonCoordName = "box";

                                    finalDescription.AppendFormat("Rule 2: Pencilmark {0} has color {1} in cell {2} and also in cell " +
                                        "{3}.  The number can only appear once in the {4}.  Therefore this color is the \"false\" color.",
                                        singleColorPM, pm1Color, csvColorCell, csvColorCell2, commonCoordName);

                                    FalseColorProcessing(colorizedCells, finalDescription, pm2Color);
                                    medusaRuleFound = true;
                                    break;
                                }
                            }
                        }

                        if (medusaRuleFound)
                            break;

                        #endregion

                        #region Medusa - Rule 3: Extra pencilmarks in 2 color cells

                        List<Cell> extraPMCells = colorizedCells
                            .Where(cell => cell.PencilMarkCount > 2 && cell.ColorizedPencilmarkCount == 2)
                            .ToList();

                        if (extraPMCells.Count > 0)
                        {
                            extraPMCells.ForEach(cell => cell.MarkPencilmarksForRemoval(
                                cell.PMUncolorizedSignature
                                    .ListOfBits()
                                    .ToArray()));

                            string csvExtraPMs = CellsInCsvFormat(extraPMCells);
                            finalDescription.AppendFormat("Rule 3: Cells {0} include pencilmarks with both colors. Therefore " +
                                "Any other pencilmark in these cells can be removed.\r\n", csvExtraPMs);

                            medusaRuleFound = true;
                        }

                        #endregion

                        #region Medusa - Rule 4: Uncolorized pm shadowed by 2 pm's of different colors

                        var Color1CellsByPM = new Dictionary<int, List<Cell>>();
                        var Color2CellsByPM = new Dictionary<int, List<Cell>>();

                        foreach (Cell cell in colorizedCells)
                        {
                            foreach (int colorizedPM in cell.ListOfColorizedPencilmarks())
                            {
                                PMColor pmColor = cell.GetPencilmarkColor(colorizedPM);
                                Dictionary<int, List<Cell>> workDictionary = (pmColor == PMColor.COLOR1) ? Color1CellsByPM : Color2CellsByPM;
                                if (!workDictionary.ContainsKey(colorizedPM))
                                    workDictionary.Add(colorizedPM, new List<Cell>());

                                workDictionary[colorizedPM].Add(cell);
                            }
                        }

                        List<int> searchPMs = Color1CellsByPM.Keys.Intersect(Color2CellsByPM.Keys).ToList();
                        affectedCells = new List<Cell>();
                        foreach (int singleSearchPM in searchPMs)
                        {
                            List<Cell> cellsWithPMColor1 = Color1CellsByPM[singleSearchPM];
                            List<Cell> cellsWithPMColor2 = Color2CellsByPM[singleSearchPM];
                            bool firstItemFound = false;
                            foreach (Cell cell1 in cellsWithPMColor1)
                                foreach (Cell cell2 in cellsWithPMColor2)
                                {
                                    if (cell1.CanSee(cell2))
                                        continue;

                                    List<Cell> solutionCells = cell1.ShadowedCells
                                        .Intersect(cell2.ShadowedCells)
                                        .Where(cell => cell.IsPencilmarkSet(singleSearchPM) &&
                                            cell.GetPencilmarkColor(singleSearchPM) == PMColor.NONE)
                                        .ToList();

                                    solutionCells.ForEach(cell => cell.MarkPencilmarksForRemoval(singleSearchPM));
                                    affectedCells.AddRange(solutionCells);

                                    if (solutionCells.Count > 0)
                                    {
                                        if (!firstItemFound)
                                        {
                                            finalDescription.AppendFormat("Rule 4: When a pencilmark is shadowed by 2 colorized " +
                                                "pencilmarks in 2 different colors and the same value, the pencilmark is not a solution " +
                                                "for the cell.");

                                            firstItemFound = true;
                                            medusaRuleFound = true;
                                        }

                                        solutionCells.ForEach(cell => finalDescription.AppendFormat("Remove pencilmark {0} from cell {1};",
                                            singleSearchPM, CellsInCsvFormat(cell)));
                                    }
                                }
                        }

                        if (affectedCells.Count > 0)
                            finalDescription.Append("\r\n");

                        #endregion

                        #region Medusa - Rule 5: Non-Colorized PM sharing colorizedPM in cell can see same value colorizedPM somewhere else

                        // The rule goes like this:  There are 2 candidates in a cell: cA and cB. cB has been colorized "Blue".   
                        // If cA can see another cA value PM with color "Green" somewhere else, then cA can be eliminated in the 
                        // original cell.  Example:  Cell [5,5] has blue pencilmark for "7", and no color pencilmark "1".   Cell [5,6]
                        // has a green pencilmark "1".  If "Green" is true, then [5, 6] will have a "1" and [5, 5] cannot be "1".  
                        // If "blue" is true, then [5, 5] will be "7" filling the value for the cell.   Either way, "1" cannot be an option
                        // for cell [5,5]

                        List<Cell> rule5Candidates = colorizedCells
                            .Where(cell => cell.ColorizedPencilmarkCount == 1 && cell.PencilMarkCount >= 2)
                            .ToList();

                        affectedCells.Clear();
                        foreach (Cell cand5Cell in rule5Candidates)
                        {
                            PMColor cellPMColor = cand5Cell.GetColorOfOnlyColorizedPM();
                            PMColor searchedColor = cellPMColor.ToggleColor();
                            List<int> noColorPMs = cand5Cell.PMSignature
                                .ListOfBits()
                                .Except(cand5Cell.ListOfColorizedPencilmarks())
                                .ToList();

                            List<Cell> cellScope = cand5Cell.ShadowedCells.ToList();
                            foreach (int singlePM in noColorPMs)
                            {
                                List<Cell> rule5Cells = cellScope.Where(cell => cell.IsPencilmarkSet(singlePM) &&
                                        cell.GetPencilmarkColor(singlePM) == searchedColor)
                                    .ToList();

                                if (rule5Cells.Count > 0)
                                {
                                    string csvRule5Cells = CellsInCsvFormat(rule5Cells);
                                    string csvCand5Cell = CellsInCsvFormat(cand5Cell);
                                    int colorPMInCand5Cell = cand5Cell.GetPMsWithColor(cellPMColor)[0];
                                    string cellForm = (rule5Cells.Count > 1) ? "Cells" : "Cell";
                                    string verbForm = (rule5Cells.Count > 1) ? "have" : "has";

                                    cand5Cell.MarkPencilmarksForRemoval(singlePM);
                                    finalDescription.AppendFormat("Rule 5: {6} {0} {7} pencilmark {1} colorized with {2}. " +
                                        "Cell {4} has pencilmark {1} with no color, and pencilmark {5} with color {3}. " +
                                        "Therefore, {1} cannot be a solution for cell {4}.\r\n",
                                   csvRule5Cells, singlePM, searchedColor, cellPMColor, csvCand5Cell, colorPMInCand5Cell,
                                   cellForm, verbForm);

                                    medusaRuleFound = true;
                                }
                            }
                        }
                        #endregion

                        #region Medusa - Rule 6: Cell Emptied by color. All non colored cells see same PM in one color

                        // Rule 6 works like this:   A cell with only non-colorized pencilmarks, where all pencilmarks see pencilmarks
                        // in the same color.  Then, that color is false, or else the cell would be empty.   Example:  Non-colorized
                        // cell has pencilmarks 1, 2, 5.  Cell 1 sees a yellow 1.  2 sees a yellow 2, and 5 sees a yellow 5.  If 
                        // yellow assumption was true, then there would be no possible solution for the cell.   Therefore yellow has
                        // to be the "false" color.

                        List<Cell> nonColorizedCells = AllBoardCells
                            .Where(cell => cell.Value == 0 && cell.ColorizedPencilmarkCount == 0)
                            .ToList();

                        bool foundRule6Case = false;
                        foreach (Cell nonColorCell in nonColorizedCells)
                        {
                            List<int> nonColorPMList = nonColorCell.PMSignature.ListOfBits();
                            List<Cell> shadowedCells = nonColorCell.ShadowedCells;
                            bool complies = true;
                            foreach (PMColor testColor in new[] { PMColor.COLOR1, PMColor.COLOR2 })
                            {
                                complies = true;
                                foreach (int nonColorPM in nonColorPMList)
                                {
                                    List<Cell> SameColorSamePMList = shadowedCells.Where(cell => cell.IsPencilmarkColorized(nonColorPM) &&
                                            cell.GetPencilmarkColor(nonColorPM) == testColor)
                                        .ToList();

                                    if (SameColorSamePMList.Count > 0)
                                        continue;

                                    complies = false;
                                    break;
                                }

                                if (complies)  // testColor is false
                                {
                                    finalDescription.AppendFormat("Rule 6: Cell {0} has pencilmark(s) {1}, all of which can see " +
                                        "a pencilmark with the same value colorized {2}.  This color has to be \"false\" or else the " +
                                        " cell would be empty.", CellsInCsvFormat(nonColorCell), PMsInCsvFormat(nonColorPMList),
                                        testColor);

                                    FalseColorProcessing(colorizedCells, finalDescription, testColor);
                                    foundRule6Case = true;
                                    medusaRuleFound = true;
                                    break;
                                }
                            }

                            if (foundRule6Case)
                                break;
                        }

                        #endregion

                    } while (false);

                    if (medusaRuleFound)
                    {
                        Solution = new SolveInfo();
                        Solution.Description = $"3D Medussa Rules\r\n{finalDescription.ToString()}";
                        foreach (Cell cell in colorizedCells)
                        {
                            var pmFindings = new List<PMFinding>();
                            cell.ListOfColorizedPencilmarks()
                                .ForEach(colorPM => pmFindings.Add(
                                    new PMFinding(colorPM, (cell.GetPencilmarkColor(colorPM) == PMColor.COLOR1) ?
                                        PMRole.ChainColor1 : PMRole.ChainColor2)));

                            cell.ListOfPMsForRemoval.ForEach(removePM => pmFindings.Add(
                                new PMFinding(removePM, PMRole.Remove)));

                            CellRole cellRole = CellRole.Pattern;
                            if (cell.IsPencilmarkColorized(pm) && cell.GetPencilmarkColor(pm) == PMColor.COLOR1)
                                cellRole = CellRole.Pattern2;

                            Solution.AddAction(cell, cellRole, pmFindings.ToArray());
                        }

                        AllBoardCells
                            .Except(colorizedCells)
                            .Where(cell => cell.HasPencilmarksMarkedForRemoval)
                            .ToList()
                            .ForEach(cell => Solution.AddAction(cell, CellRole.Affected, 
                                GetPmFindings(cell, cell.ListOfPMsForRemoval.ToBitMask(), PMRole.Remove)));

                        return true;
                    }

                    #endregion
                }
            }

            return false;
        }

        private void FalseColorProcessing(List<Cell> colorizedCells, StringBuilder finalDescription, PMColor falseColor)
        {
            PMColor trueColor = falseColor.ToggleColor();

            // If the cell has 2 colors, the solution is the true color.   If the cell has only the duplicate color,
            // then duplicate colors can be eliminated since they are not possible solutions.
            List<Cell> solvedCells = colorizedCells
                .Where(cell => cell.ColorizedPencilmarkCount == 2)
                .ToList();

            if (solvedCells.Count > 0)
            {
                string csvSolvedCells = CellsInCsvFormat(solvedCells);
                finalDescription.AppendFormat("Rule 1: Cells {0} have pencilmarks colorized in 2 colors. Any non-{1} " +
                    "color pencilmark will not be a solution to the cell.\r\n", csvSolvedCells, trueColor);

                foreach (Cell cell in solvedCells)
                {
                    int[] pmsForRemoval = cell
                        .PMSignature
                        .ListOfBits()
                        .Except(cell.GetPMsWithColor(trueColor))
                        .ToArray();

                    cell.MarkPencilmarksForRemoval(pmsForRemoval);
                }
            }

            // Final step:  Any cell having a pencilmark colored the duplicate color, can have that pencilmark
            // removed as a candidate.
            var falseCells = new StringBuilder();
            foreach (Cell colorCell in colorizedCells)
            {
                if (colorCell.ColorizedPencilmarkCount != 1 || colorCell.GetColorOfOnlyColorizedPM() != falseColor)
                    continue;

                int falsePM = colorCell.GetPMsWithColor(falseColor)[0];
                colorCell.MarkPencilmarksForRemoval(falsePM);
                falseCells.AppendFormat("Pencilmark {0} in cell {1} - ", falsePM, CellsInCsvFormat(colorCell));
            }

            if (falseCells.Length > 0)
            {
                falseCells.Length -= 3;  // remove final " - "
                finalDescription.AppendFormat("All pencilmarks colorized with {0} cannot be solutions " +
                    "for their cells: {1}.\r\n", falseColor, falseCells.ToString());
            }

        }

        /// <summary>
        /// Creates a list of segments for all the conjugate pairs in the board.  A
        /// conjugate pair is a line, column or box where a given pencilmark appears only
        /// in 2 cells.
        /// </summary>
        /// <param name="pencilmark">pencilmark for which the coloring will happen.</param>
        private void GetConjugateSegments(int pencilmark)
        {
            Board.ClearPMColorization();
            CellGraph = new Dictionary<Cell, List<Cell>>();
            VisitedCells = new List<Cell>();
            foreach (List<List<Cell>> singleGroup in CellLineGroups)
            {
                foreach (List<Cell> line in singleGroup)
                {
                    List<Cell> cellsWithPMInLine = line
                        .Where(cell => cell.IsPencilmarkSet(pencilmark))
                        .ToList();

                    if (cellsWithPMInLine.Count != 2)
                        continue;

                    if (AddPathSegmentToCellGraph(cellsWithPMInLine))
                    {
                        Logger.WriteLine("Found segment {0} with pencilmark {1}.",
                            CellsInCsvFormat(cellsWithPMInLine), pencilmark);
                    }
                }
            }
        }

        /// <summary>
        /// Method to get the conjugates for a cell on a given pencilmark.  A conjugate is 
        /// another cell in the same line with the same pencilmark set.   To be a conjugate
        /// the line (row, column or group) must have only 2 cells with that pencilmark set.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="pencilmark"></param>
        /// <returns></returns>
        private List<Cell> GetConjugates(Cell cell, int pencilmark)
        {
            List<List<Cell>> workLines = new List<List<Cell>>
            {
                CellLineGroups[INDEX_ROWS][cell.RowIndex],
                CellLineGroups[INDEX_COLUMNS][cell.ColIndex],
                CellLineGroups[INDEX_SECTIONS][cell.GroupIndex]
            };

            var returnValue = new List<Cell>();
            foreach (List<Cell> line in workLines)
            {
                List<Cell> cellsWithPMSet = line
                    .Where(lineCell => lineCell != cell && lineCell.IsPencilmarkSet(pencilmark))
                    .ToList();

                if (cellsWithPMSet.Count != 1)
                    continue;

                returnValue.AddUnique(cellsWithPMSet[0]);
            }

            return returnValue;
        }

        private bool AddPathSegmentToCellGraph(List<Cell> segment)
        {
            bool returnValue = false;
            for (int i = 0; i < 2; i++)
            {
                Cell cell = segment[i];
                Cell otherCell = segment[1 - i];
                if (!CellGraph.ContainsKey(cell))
                {
                    CellGraph.Add(cell, new List<Cell>());
                }

                if (!CellGraph[cell].Contains(otherCell))
                {
                    CellGraph[cell].Add(otherCell);
                    returnValue = true;
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Method to take the list of conjugate segments and colorize them according to the
        /// connections found.   The segments can form a single connected graph, or several 
        /// independent graphs not connected to each other.
        /// <param name="pencilmark">Pencilmark being colorized in all the segments</param>
        /// </summary>
        protected void ColorizeSegments(int pencilmark)
        {
            PMColor[] workColors = new[] { PMColor.COLOR1, PMColor.COLOR2 };
            int currentColorIndex = 1;  // will be toggled before first colorization
            Queue<Cell> processingQueue = new Queue<Cell>();
            Queue<Cell> graphQueue = new Queue<Cell>(CellGraph.Keys);
            int queueCount = 0;
            Logger.WriteLine("Will start colorizing.  CellGraph is {0}.", CellGraph.Keys);

            while ((queueCount = processingQueue.Count) > 0 || graphQueue.Count > 0)
            {
                Cell workCell;
                if (queueCount == 0)
                {
                    workCell = graphQueue.Dequeue();
                    if (workCell.IsPencilmarkColorized(pencilmark))
                        continue;

                    if (Board.HasColorizedCells())
                        return;  // there are independent graphs for the same pencilmark

                    processingQueue.Enqueue(workCell);
                    processingQueue.Enqueue(null);
                    currentColorIndex ^= 1;

                    Logger.WriteLine("Enqueued cell {0} followed by null separator.",
                        CellsInCsvFormat(workCell));

                    continue;
                }

                workCell = processingQueue.Dequeue();
                if (workCell == null)
                {
                    currentColorIndex ^= 1;
                    if (processingQueue.Count > 0)
                        processingQueue.Enqueue(null);

                    continue;
                }

                if (!workCell.IsPencilmarkColorized(pencilmark))
                {
                    workCell.ColorizePencilmark(pencilmark, workColors[currentColorIndex]);
                    CellGraph[workCell].ForEach(cell => processingQueue.Enqueue(cell));
                    VisitedCells.Add(workCell);

                    Logger.WriteLine("Cell {0} colorized with {1} - Connected cells {2} added to queue.",
                        CellsInCsvFormat(workCell), workColors[currentColorIndex],
                            CellsInCsvFormat(CellGraph[workCell].ToList(), false/*sorted*/));

                    continue;
                }

                PMColor currentColor = workColors[currentColorIndex];
                if (workCell.GetPencilmarkColor(pencilmark) != currentColor)
                {

                    if (ContradictionColor == currentColor)
                    {
                        Logger.WriteLine("Found cell {0} with color {1} when assigning color {2}",
                            CellsInCsvFormat(workCell), workColors[currentColorIndex ^ 1], currentColor);

                        throw new UnexpectedStateException("Both colors contradicted when processing a simple color chain.");
                    }

                    ContradictionColor = workColors[currentColorIndex ^ 1];
                    continue;
                }
            }
        }

        protected void ColorizeMedusa(int pencilmark)
        {
            List<Cell> colorizedCells = Board
                .GetAllColorizedCells()
                .ToList();

            var processingQueue = new Queue<Tuple<Cell, int, PMColor>>();

            colorizedCells
                .Where(cell => cell.PencilMarkCount == 2)
                .ToList()
                .ForEach(cell => processingQueue.Enqueue(
                    new Tuple<Cell, int, PMColor>(cell, pencilmark,
                        cell.GetPencilmarkColor(pencilmark).ToggleColor())));

            while (processingQueue.Count > 0)
            {
                Tuple<Cell, int, PMColor> tuple = processingQueue.Dequeue();
                Cell colorCell = tuple.Item1;
                int lastColorizedPM = tuple.Item2;
                PMColor colorToApply = tuple.Item3;
                PMColor nextLinkColor;

                if (!colorCell.IsPencilmarkColorized(lastColorizedPM))
                {
                    if (colorCell.ColorizePencilmarkWithClashDetection(lastColorizedPM, colorToApply))
                    {
                        return;
                    }

                    nextLinkColor = colorToApply.ToggleColor();
                    GetConjugates(colorCell, lastColorizedPM)
                        .ForEach(cell => processingQueue.Enqueue(
                            new Tuple<Cell, int, PMColor>(cell, lastColorizedPM, nextLinkColor)));
                }

                if (colorCell.PencilMarkCount != 2 || colorCell.ColorizedPencilmarkCount != 1)
                    continue;

                int pmLink = colorCell.PMUncolorizedSignature.ListOfBits()[0];
                nextLinkColor = colorCell.GetColorOfOnlyColorizedPM();
                PMColor linkColor = nextLinkColor.ToggleColor();
                colorCell.ColorizePencilmark(pmLink, linkColor);

                GetConjugates(colorCell, pmLink)
                    .ForEach(cell => processingQueue.Enqueue(
                        new Tuple<Cell, int, PMColor>(cell, pmLink, nextLinkColor)));
            }
        }
    }
}
