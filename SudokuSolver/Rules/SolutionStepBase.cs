namespace SudokuSolver.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using GameParts;
    using GameParts.General;
    using RuleData;
    using Interfaces;
    using System.Text;

    public abstract class SolutionStepBase : ISolutionStep
    {
        protected GameBoard Board { get; set; }
        protected Logger Logger;

        public SolutionStepBase()
        {
            Logger = new Logger();
        }

        protected void ValidatePatternFound()
        {
            if (Solution == null)
                throw new InvalidOperationException("FindPattern did not find any solution patterns using this rule.");
        }

        #region ISolutionStep implementation
        public abstract bool FindPattern(GameBoard board);
        public SolveInfo Solution { get; protected set; }

        public virtual void Apply() { UndoRedo(false/*undo*/); }
        public virtual void Undo() { UndoRedo(true/*undo*/); }

        #endregion


        public void UndoRedo(bool undo)
        {
            ValidatePatternFound();
            Solution.Actions.ForEach(action =>
                action.PencilmarkDataList
                    .Where(pmFinding => pmFinding.Role == PMRole.Remove)
                    .ToList()
                    .ForEach(pmf => action.Cell.SetPencilMark(pmf.Value, undo/*pencilMarkActive*/)));
        }

        /// <summary>
        /// Provide the list of cells in human readable format
        /// </summary>
        /// <param name="cellList"></param>
        /// <returns></returns>
        public static string CellsInCsvFormat(IEnumerable<Cell> cellList, bool sort = true)
        {
            var sb = new StringBuilder();
            List<Cell> orderedList = sort ? 
                cellList
                    .OrderBy(cell => cell.RowIndex * 1000 + cell.ColIndex)
                    .ToList()
                :
                cellList.ToList();
            
            orderedList.ForEach(cell => sb.AppendFormat("[{0},{1}], ",
                cell.RowIndex + 1, cell.ColIndex + 1));

            if (sb.Length > 0)
                sb.Length -= 2;

            return sb.ToString();
        }

        public static string CellsInCsvFormat(params Cell[] cells)
        {
            string returnValue = CellsInCsvFormat(new List<Cell>(cells));
            return returnValue;
        }

        public static string PMsInCsvFormat(List<int> pmList)
        {
            string returnValue = string.Join(", ", pmList.Select(t => t.ToString()));
            return returnValue;
        }

        protected PMFinding[] GetPmFindings(Cell cell, int pmMask, PMRole pmRole)
        {
            PMFinding[] returnValue = (cell.PMSignature & pmMask).ListOfBits()
                .Select(pm => new PMFinding(pm, pmRole))
                .ToArray();

            return returnValue;
        }

        protected PMFinding[] GetPmFindings(Cell cell, int[] pmMasks, PMRole[] pmRoles)
        {
            int len = pmMasks.Length;
            var returnValue = new List<PMFinding>();
            for (int i = 0; i < len; i++)
            {
                PMFinding[] groupPM = GetPmFindings(cell, pmMasks[i], pmRoles[i]);
                returnValue.AddRange(groupPM);
            }

            return returnValue.ToArray();
        }
    
    }
}
