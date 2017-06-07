namespace SudokuSolver.Rules
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using GameParts;
    using RuleData;

    public class HiddenValuesRule : HiddenNakedRuleBase
    {
        protected override CellRole AffectedCellRole { get { return CellRole.Pattern; } }

        protected override List<int> CreateMapBits(List<Cell> line)
        {
            List<int> returnValue = new int[line.Count + 1].ToList();
            for (int pm = 1; pm <= line.Count; pm++)
            {
                int cellMask = 1;
                foreach (Cell cell in line)
                {
                    returnValue[pm] |= cell.IsPencilmarkSet(pm) ? cellMask : 0;
                    cellMask <<= 1;
                }
            }
            return returnValue;
        }

        protected override void CreateRuleInformation()
        {
            int pmComplementMask = PencilmarkList.ToBitMask();
            pmComplementMask = Mask.ForSettingPencilMarks(Board.Dimension) & ~pmComplementMask;
            int pmMaskList = 0;
            affectedCells.ForEach(cell => pmMaskList |= (cell.PMSignature & pmComplementMask));

            List<int> pmList = pmMaskList.ListOfBits();
            string csvPattern = CellsInCsvFormat(patternCells);
            string csvAffected = CellsInCsvFormat(affectedCells);
            string csvPencilmarks = PMsInCsvFormat(PencilmarkList);
            string csvOtherPMs = PMsInCsvFormat(pmList);

            bool multiCell = patternCells.Count > 1;
            bool multiPM = PencilmarkList.Count > 1;
            string cellPlurality = (multiCell) ? "s" : string.Empty;
            string cellArticle = (multiCell) ? "these" : "this";
            string valuePlurality = (multiPM) ? "s" : string.Empty;
            string valueReference = (multiPM) ? "they do" : "it does";
            string optionPlurality = (pmList.Count > 1) ? "s" : string.Empty;

            Solution.Description = string.Format("Hidden Values Rule: Cell{0} {1} must have value{2} {3}, since " +
                "{7} not appear anywhere else in the  {8}.  Therefore option{4} {5} can be removed from {6} cell{0}.",
                cellPlurality, csvPattern, valuePlurality, csvPencilmarks, optionPlurality, csvOtherPMs, 
                cellArticle, valueReference, Coord.Name(LineCoordinate));
        }

        internal override List<int> GetLocationList(NakedHiddenFinder hiddenFinder)
        {
            int mask = 0;
            hiddenFinder.ActionIndexes
                .Select(i => hiddenFinder.MapBits[i])
                .ToList()
                .ForEach(pmsign => mask |= pmsign);

            List<int> returnValue = mask.ListOfBits();
            return returnValue;
        }

        internal override List<int> GetPencilmarkList(NakedHiddenFinder hiddenFinder)
        {
            return hiddenFinder.ActionIndexes;
        }

        protected override int GetPencilmarkMask()
        {
            int returnValue = (Mask.ForSettingPencilMarks(Board.Dimension) & ~PencilmarkList.ToBitMask());
            return returnValue;
        }

        protected override void CalculateAffectedCells(List<Cell> line)
        {
            int pmMask = (Mask.ForSettingPencilMarks(Board.Dimension) & ~PencilmarkList.ToBitMask());
            affectedCells = patternCells
                .Where(t => (t.PMSignature & pmMask) != 0)
                .ToList();
        }
    }
}
