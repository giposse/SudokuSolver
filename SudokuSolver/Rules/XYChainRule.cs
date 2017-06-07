namespace SudokuSolver.Rules
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using GameParts;
    using RuleData;

    public class XYChainRule : SolutionStepBase
    {
        private List<Cell> Chain;
        private List<Cell> affectedCells;

        public override bool FindPattern(GameBoard board)
        {
            Board = board;
            Chain = new List<Cell>();
            List<Cell> allXYChainCandidates = Board.GetAllLines(Coord.ROWS)
                .SelectMany(t => t.Where(cell => cell.PMSignature.NumberOfBitsSet() == 2))
                .ToList();

            Logger.WriteLine("XY candidates cells are {0}.", CellsInCsvFormat(allXYChainCandidates));
            Cell dummyCell = allXYChainCandidates
                .FirstOrDefault(cell => HasXYChain(cell, 0/*nextPmMask*/, 0/*lastPM*/));

            return affectedCells != null;
        }

        private bool HasXYChain(Cell cell, int nextPmMask, int lastPM)
        {
            if (Chain.Contains(cell))
                return false;

            int chainCount = Chain.Count;
            int lastPmMask = 1 << lastPM;
            Chain.Add(cell);
            if (chainCount == 0)
                nextPmMask = cell.PMSignature;

            Logger.WriteLine("Chain is {0}.", CellsInCsvFormat(Chain, false/*sorted*/));

            int chainLength = Chain.Count;
            Cell chainEnd;
            do
            {
                if (chainLength == 2)
                {
                    int headSignature = Chain[0].PMSignature;
                    int pmDiffs = headSignature ^ cell.PMSignature;
                    List<int> lastPMList = (pmDiffs & headSignature).ListOfBits();
                    if (lastPMList.Count != 1)
                        continue;

                    lastPM = lastPMList[0];
                    lastPmMask = 1 << lastPM;
                    nextPmMask = pmDiffs & cell.PMSignature;
                }
                else if (chainLength >= 4 && (chainLength & 0x01) == 0 && nextPmMask == lastPmMask)
                {
                    affectedCells = Chain[0].ShadowedCells
                        .Intersect(cell.ShadowedCells)
                        .Except(Chain)
                        .Where(c => c.IsPencilmarkSet(lastPM))
                        .ToList();

                    if (affectedCells.Count == 0)
                    {
                        affectedCells = null;
                        continue;
                    }

                    Solution = new SolveInfo();
                    affectedCells.ForEach(affCell => Solution.AddAction(affCell, CellRole.Affected,
                        GetPmFindings(affCell, lastPmMask, PMRole.Remove)));

                    int last = Chain.Count - 1;

                    AddSolutionAction(Chain[0], lastPmMask, PMRole.ChainEnd, PMRole.ChainColor2);
                    AddSolutionAction(Chain[last], lastPmMask, PMRole.ChainEnd, PMRole.ChainColor2);

                    PMRole[] linkRoles = new[] { PMRole.ChainColor1, PMRole.ChainColor2 };
                    int linkIndex = 1;
                    int lastMask = lastPmMask;
                    int nextMask = Chain[0].PMSignature ^ lastMask;
                    for (int iCell = 1; iCell < last; iCell++)
                    {
                        Cell chainCell = Chain[iCell];
                        int currentMask = chainCell.PMSignature ^ nextMask;
                        Solution.AddAction(chainCell, CellRole.Pattern, 
                            GetPmFindings(chainCell, 
                            new int[] { nextMask, currentMask },
                            new PMRole[] { linkRoles[linkIndex], linkRoles[1 - linkIndex] }));

                        nextMask = currentMask;
                        linkIndex = 1 - linkIndex;
                    }


                    string csvPattern = CellsInCsvFormat(Chain, false/*sort*/);
                    string csvAffected = CellsInCsvFormat(affectedCells);
                    string csvHead = CellsInCsvFormat(Chain[0]);
                    string csvLast = CellsInCsvFormat(cell);

                    Solution.Description = string.Format("XY-Chain Rule:  Cells {0} form a chain, jumping from one to " +
                        "the next using a common pencilmark.  One of the ends of the chain (either {1} or {2}) will " +
                        "have value {3}. Therefore {4} cannot have value {3}.",
                        csvPattern, csvHead, csvLast, lastPM, csvAffected);

                    return true;
                }

                chainEnd = cell.ShadowedCells
                    .FirstOrDefault(c => c.PMSignature.NumberOfBitsSet() == 2 &&
                        (c.PMSignature & nextPmMask) != 0 &&
                        HasXYChain(c, (c.PMSignature ^ nextPmMask), lastPM));

                if (chainEnd != null)
                    return true;

            } while(false);

            Logger.WriteLine("Trimming Chain to length {0}.", chainCount);
            Chain.Trim(chainCount);
            return false;
        }

        private void AddSolutionAction(Cell cell, int mask, PMRole maskRole, PMRole nextRole)
        {
            int otherMask = cell.PMSignature ^ mask;
            Solution.AddAction(cell, CellRole.Pattern,
                GetPmFindings(cell,
                new int[] { mask, otherMask },
                new PMRole[] { maskRole, nextRole }));
        }
    }
}
