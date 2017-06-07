namespace SudokuSolver
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using GameParts;
	using Rules;
	using Interfaces;
	using RuleData;

	public static class GameSolver
    {
        private static Random random = new Random();

        private static Func<ISolutionStep>[] RuleCreator = new Func<ISolutionStep>[]
        {
            () => new SinglePossibleValue(),

            // if both Naked Value and Hidden value are used, one will never show up, so
            // they are randomized so both will show up.  Only one of the rules is necessary
            () => (random.Next(2) == 1) ? 
				(ISolutionStep) new NakedValuesRule()
				:
				(ISolutionStep) new HiddenValuesRule(),

            () => new LockedInLinesOrGroups(),
            () => new RemotePair(),
            () => new XYChainRule(),
            () => new XYZWingRule(),
           //
            () => new FishPatternsAll(2, false/*finned*/, false/*sashimi*/),  // X-Wing
            () => new FishPatternsAll(2, true/*finned*/, false/*sashimi*/),  // Finned X-Wing
            () => new FishPatternsAll(2, true/*finned*/, true/*sashimi*/),  // Sashimi X-Wing
            () => new XYWingPattern(),
            () => new UniqueRectangleRule(),
            () => new FishPatternsAll(3, false/*finned*/, false/*sashimi*/),  // Swordfish
            () => new FishPatternsAll(3, true/*finned*/, false/*sashimi*/),  // Finned Swordfish
            () => new FishPatternsAll(3, true/*finned*/, true/*sashimi*/),  // Sashimi Swordfish
            () => new FishPatternsAll(4, false/*finned*/, false/*sashimi*/),  // Jellyfish
            () => new FishPatternsAll(4, true/*finned*/, false/*sashimi*/),  // Finned Jellyfish
            () => new FishPatternsAll(4, true/*finned*/, true/*sashimi*/),  // Sashimi Jellyfish
            () => new ALSxzRule(),
            () => new SimpleAndMedusaColoring(false/*medusaOnly*/),
            () => new SimpleAndMedusaColoring(true/*medusaOnly*/),
        };

        public static List<ISolutionStep> Solve(GameBoard board, out bool couldSolve)
        {
            List<Cell> unsolved = board
                .GetAllLines(Coord.BYROWS)
                .SelectMany(t => t)
                .Where(cell => cell.Value == 0)
                .ToList();

            var returnValue = new List<ISolutionStep>();
            var totalRules = RuleCreator.Length;
            int i = 0;
            couldSolve = true;
            while (i < totalRules && unsolved.Count > 0)
            {
                for (i = 0; i < totalRules; i++)
                {
                    ISolutionStep rule = RuleCreator[i]();
                    if (!rule.FindPattern(board))
                        continue;

                    returnValue.Add(rule);
                    rule.Apply();
                    if (i == 0)  // only rule which solves the cell
                    {
                        Cell solutionCell = rule.Solution.Actions
                            .First(action => action.PencilmarkDataList
                                .FirstOrDefault(pmFinding => pmFinding.Role == PMRole.Solution) != null)
                            .Cell;

                        unsolved.Remove(solutionCell);
                    }

                    break;
                }

                if (i == totalRules)
                {
                    couldSolve = false;
                    Console.WriteLine("Could not solve the puzzle.  Still {0} cells unsolved.", unsolved.Count);
                    break;
                }
            }

            return returnValue;
        }
    }
}
