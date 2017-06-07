namespace SudokuSolver.Rules
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;

    internal class NakedHiddenFinder
    {
        public List<int> MapBits { get; set; }
        public int MapLength { get; set; }
        public List<int> ActionBits { get; private set; }
        public List<int> ActionIndexes {get; private set; }

        public bool Search(int goal, int start = 0, int bitMask = 0, List<int> indexes = null)
        {
            indexes = (indexes == null) ? new List<int>() : indexes;
            for (int i = start; i < MapLength; i++)
            {
                var cumulativeIndexes = new List<int>(indexes);
                if ((MapLength - start) < (goal - cumulativeIndexes.Count))
                    return false;

                if (MapBits[i] == 0)
                    continue;

                int cumulativeBitMask = bitMask | MapBits[i];
                int bitCount = cumulativeBitMask.NumberOfBitsSet();
                if (bitCount > goal)
                    continue;

                cumulativeIndexes.Add(i);
                if (cumulativeIndexes.Count < goal)
                {
                    if (!Search(goal, i + 1, cumulativeBitMask, cumulativeIndexes))
                        continue;
                }

                if (ActionBits == null)
                {
                    // for hidden values, this will do
                    // "Other pencilmarks having the same places"
                    // for naked values, this will do
                    // "Other places having the same pencilmarks"
                    ActionBits = MapBits
                        .Select((v, index) => new { Value = v, Index = index })
                        // "other places" for naked, or "other pencilmarks" for hidden
                        .Where(t => !cumulativeIndexes.Contains(t.Index)
                            // "having the same pencilmarks" for naked
                            //  "having the same places" for hidden
                            && (t.Value & cumulativeBitMask) != 0)
                        .Select(t => t.Value & cumulativeBitMask)
                        .ToList();

                    if (ActionBits.Count == 0)
                    {
                        ActionBits = null;
                        continue;
                    }

                    ActionIndexes = cumulativeIndexes;
                }

                return true;
            }

            return false;
        }
    }
}
