namespace SudokuSolver.Rules
{
	using System.Collections.Generic;
	using System.Linq;
	using Extensions;
	using GameParts;
	using RuleData;

	public class NakedValuesRule : HiddenNakedRuleBase
    {
        protected override CellRole AffectedCellRole { get { return CellRole.Affected; } }
        
        protected override List<int> CreateMapBits(List<Cell> line)
        {
            List<int> returnValue = CreateMapBitsFromLine(line);
            return returnValue;
        }

        internal static List<int> CreateMapBitsFromLine(List<Cell> line)
        {
            List<int> returnValue = line.Select(cell => cell.PMSignature).ToList();
            return returnValue;
        }

        protected override void CreateRuleInformation()
        {
            string csvPattern = CellsInCsvFormat(patternCells);
            string csvAffected = CellsInCsvFormat(affectedCells);
            string csvPencilmarks = PMsInCsvFormat(PencilmarkList);
            int patternPMs = 0;
            patternCells.ForEach(cell => patternPMs |= cell.PMSignature);
            string csvPatternPMs = PMsInCsvFormat(patternPMs.ListOfBits());
            bool singlePM = PencilmarkList.Count == 1;
            bool singleCell = affectedCells.Count == 1;

            Solution.Description = string.Format("Naked Values Rule: Cells {0} must contain values {1}." +
                "Therefore value{3} {6} can be removed from cell{4} {5}.", 
                csvPattern, csvPatternPMs, 
                singlePM ? "this" : "these",
                singlePM? string.Empty : "s", 
                singleCell ? string.Empty : "s", 
                csvAffected,
                csvPencilmarks
                );
        }

        internal override List<int> GetLocationList(NakedHiddenFinder hiddenFinder)
        {
            return hiddenFinder.ActionIndexes;
        }

        internal override List<int> GetPencilmarkList(NakedHiddenFinder hiddenFinder)
        {
            List<int> returnValue = hiddenFinder.ActionBits
                .BitMerge()
                .ListOfBits();

            return returnValue;
        }

        protected override int GetPencilmarkMask()
        {
            int returnValue = PencilmarkList.ToBitMask();
            return returnValue;
        }

        protected override void CalculateAffectedCells(List<Cell> line)
        {
            int pmMask = PencilmarkList.ToBitMask();

            affectedCells = line.Except(patternCells)
                .Where(t => (t.PMSignature & pmMask) != 0)
                .ToList();
        }
    }
}
