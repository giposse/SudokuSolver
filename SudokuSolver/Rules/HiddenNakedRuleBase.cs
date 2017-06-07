namespace SudokuSolver.Rules
{
    using System.Collections.Generic;
    using System.Linq;
    using GameParts;
    using Extensions;
    using RuleData;

    public abstract class HiddenNakedRuleBase : SolutionStepBase
    {
        protected List<int> PencilmarkList;
        protected List<Cell> affectedCells;
        protected List<Cell> patternCells;

        protected abstract List<int> CreateMapBits(List<Cell> line);
        protected abstract void CreateRuleInformation();
        internal abstract List<int> GetLocationList(NakedHiddenFinder hiddenFinder);
        internal  abstract List<int> GetPencilmarkList(NakedHiddenFinder hiddenFinder);
        protected abstract void CalculateAffectedCells(List<Cell> line);
        protected abstract int GetPencilmarkMask();
        protected abstract CellRole AffectedCellRole { get; }
        protected int LineCoordinate;

        public override bool FindPattern(GameBoard board)
        {
            // The algorithm is the same for finding naked or hidden values.  For naked:
            // an array of integers where each integer is a set of bits for the pencilmarks.
            // (e.g. 01 0110 0010 == 0x162 which mean pencilmark 1, 5, 6, and 8 are set.  The
            // LSB is position 0), and the index in the array is a cell position in the line.
            // For Hidden:  Swap the meaning: The bits are the positions in the line and the 
            // index is the pencilmark.
            // Start from the beginning in the array.   Its content is a cumulative mask.
            // If the cumulative mask has more than the goal bits, it is thrown away and the
            // next one is considered.   If N bits are found in a mask using N entries in the
            // array, then there are N of N which is what is being searched for.   Once found,
            // a "final approval method" is invoked to check if the set is good.  The approval
            // is determined to be good if there are pencilmarks to turn off. (for naked, in 
            // other positions in the line.  for hidden: more pencilmarks in the same cells 
            // (other than the N found (e.g. found 1,2,3 but cell also has 2,7 which can be
            // turned off.  Since there is a large number of combinations the best approach
            // to try promising ones and discard useless ones quickly is to use recursion.
            // Since a board has a small number of cells (9 or so) the recursion depth is not
            // an issue.

            Board = board;
            for (int coord = Coord.ROWS; coord <= Coord.SECTIONS; coord++)
            {
                List<List<Cell>> allLines = Board.GetAllLines(coord);
                foreach (List<Cell> line in allLines)
                {
                    var hiddenFinder = new NakedHiddenFinder();
                    int dim = line.Count + 1;
                    hiddenFinder.MapBits = CreateMapBits(line);
                    hiddenFinder.MapLength = hiddenFinder.MapBits.Count;

                    // Step 2:  Search for N of N values
                    int maxGoal = hiddenFinder.MapBits.Count(t => t != 0);
                    if (maxGoal == 0)
                        continue;

                    int goal = 1;
                    for (; goal < maxGoal; goal++)
                        if (hiddenFinder.Search(goal))
                            break;

                    // Finally set the information as related to cells, and information
                    if (goal == maxGoal)
                        continue;

                    LineCoordinate = coord;
                    int indexLine = line[0].GetCoordinate(LineCoordinate);
                    List<int> locationList = GetLocationList(hiddenFinder);
                    patternCells = locationList.Select(i => line[i]).ToList();

                    PencilmarkList = GetPencilmarkList(hiddenFinder);
                    Solution = new SolveInfo();
                    int pmMask = GetPencilmarkMask();

                    int patternPMs = 0;
                    patternCells.ForEach(cell => patternPMs |= cell.PMSignature);

                    locationList
                        .Select(loc => line[loc])
                        .ToList()
                        .ForEach(cell => Solution.AddAction(cell, CellRole.Pattern,
                            GetPmFindings(cell, patternPMs, PMRole.Pattern)));

                    List<int> solutionPMs = pmMask.ListOfBits();
                    CalculateAffectedCells(line);
                    affectedCells.ForEach(affCell =>
                        Solution.AddAction(affCell, AffectedCellRole,
                            GetPmFindings(affCell,
                                new int[] { pmMask, patternPMs },
                                new PMRole[] { PMRole.Remove, PMRole.Pattern })));

                    line
                        .Except(patternCells)
                        .Except(affectedCells)
                        .ToList()
                        .ForEach(cell => Solution.AddAction(cell, CellRole.InvolvedLine));

                    CreateRuleInformation();
                    return true;
                }
            }

            return false;
        }
    }
}
